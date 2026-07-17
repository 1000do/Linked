import logging
import time
import tempfile
import os
import math
from typing import Tuple, List, Dict, Any, AsyncIterator
import cv2
import numpy as np
from difflib import SequenceMatcher

from core.exceptions import FileProcessingException
from .base_extractor import ITextExtractor
from providers.ml_model_provider import MLModelProvider

logger = logging.getLogger(__name__)

class VideoTextExtractor(ITextExtractor):
    """Extract text from video frames and audio using OCR and Whisper."""
    
    def __init__(self, model_provider: MLModelProvider):
        self.model_provider = model_provider

    def _detect_target_frames(self, video_path: str, max_frames: int, sample_interval_seconds: float) -> Tuple[List[int], int]:
        try:
            import scenedetect
            from scenedetect import ContentDetector
            scene_list = scenedetect.detect(video_path, ContentDetector(threshold=27.0), show_progress=False)
            logger.info(f"Scenes detected: {len(scene_list)}")
            
            cap = cv2.VideoCapture(video_path)
            fps = cap.get(cv2.CAP_PROP_FPS) or 30.0
            frame_count = int(cap.get(cv2.CAP_PROP_FRAME_COUNT))
            cap.release()
            
            target_frames = []
            for scene in scene_list:
                start_frame = scene[0].frame_num
                end_frame = scene[1].frame_num
                middle_frame = start_frame + (end_frame - start_frame) // 2
                target_frames.append(middle_frame)
                
            if len(target_frames) > max_frames:
                # Pick the longest scenes
                scenes_with_duration = [(scene[1].frame_num - scene[0].frame_num, idx) for idx, scene in enumerate(scene_list)]
                scenes_with_duration.sort(reverse=True, key=lambda x: x[0])
                top_indices = [x[1] for x in scenes_with_duration[:max_frames]]
                top_indices.sort() # Keep chronological order
                target_frames = [target_frames[i] for i in top_indices]
            elif len(target_frames) == 0:
                # Fallback if no scenes detected
                total_duration = frame_count / fps if fps > 0 else 0
                interval = max(sample_interval_seconds, total_duration / max_frames if max_frames > 0 else sample_interval_seconds)
                sample_every_n_frames = max(1, int(fps * interval))
                target_frames = list(range(0, frame_count, sample_every_n_frames))[:max_frames]
                
            return target_frames, frame_count
        except Exception as e:
            logger.error(f"Scene detection failed: {e}")
            return [], 0

    def _process_frame_ocr(self, frame: np.ndarray, join_char: str = " ") -> Tuple[str, float]:
        rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        
        # Downscale for faster OCR
        height, width = rgb_frame.shape[:2]
        max_width = 800
        if width > max_width:
            scale = max_width / width
            new_width = max_width
            new_height = int(height * scale)
            rgb_frame = cv2.resize(rgb_frame, (new_width, new_height))
        
        ocr_reader = self.model_provider.get_ocr_reader()
        results = ocr_reader.readtext(rgb_frame, detail=1, paragraph=False)
        frame_texts = []
        frame_confs = []
        for res in results:
            if len(res) == 3:
                _, t, prob = res
                if t.strip():
                    frame_texts.append(t)
                    frame_confs.append(float(prob))
                    
        text = join_char.join(frame_texts).strip()
        frame_conf = sum(frame_confs) / len(frame_confs) if frame_confs else 0.0
        return text, frame_conf

    def _extract_audio_transcription(self, video_path: str) -> List[Tuple[str, float, Dict[str, Any]]]:
        audio_path = video_path.replace(".mp4", ".wav")
        results = []
        try:
            # Extract as 16kHz mono WAV for optimal Whisper quality
            os.system(f"ffmpeg -i {video_path} -vn -acodec pcm_s16le -ar 16000 -ac 1 -y {audio_path} 2>/dev/null")
            
            if os.path.exists(audio_path):
                whisper_model = self.model_provider.get_whisper_model()
                if whisper_model:
                    segments, _ = whisper_model.transcribe(
                        audio_path,
                        beam_size=5,
                        condition_on_previous_text=False
                    )
                    
                    for idx, segment in enumerate(segments):
                        seg_text = segment.text.strip()
                        if seg_text:
                            results.append((
                                seg_text,
                                math.exp(segment.avg_logprob),
                                {
                                    "source": "video_whisper_segment",
                                    "success": True,
                                    "segment_start": segment.start,
                                    "segment_end": segment.end,
                                    "text_length": len(seg_text)
                                }
                            ))
                            logger.info(f'Audio transcription on Segment {idx+1}: {seg_text}')
        except Exception as e:
            logger.warning(f"Audio extraction failed: {e}")
        finally:
            if os.path.exists(audio_path):
                try:
                    os.unlink(audio_path)
                except Exception:
                    pass
        return results

    async def extract_legacy(self, content: bytes, sample_interval_seconds: float = 3.0, max_frames: int = 30) -> Tuple[List[str], float, Dict[str, Any]]:
        if not content:
            raise FileProcessingException("video", "Empty video provided")
            
        tmp_path = None
        try:
            start_time = time.time()
            with tempfile.NamedTemporaryFile(suffix=".mp4", delete=False) as tmp:
                tmp.write(content)
                tmp_path = tmp.name
                
            all_text = []
            all_confidences = []
            frames_processed = 0
            frame_count = 0

            try:
                target_frames, frame_count = self._detect_target_frames(tmp_path, max_frames, sample_interval_seconds)
                
                cap = cv2.VideoCapture(tmp_path)
                last_text = ""
                
                for frame_idx in target_frames:
                    cap.set(cv2.CAP_PROP_POS_FRAMES, frame_idx)
                    ret, frame = cap.read()
                    if not ret:
                        continue
                        
                    text, frame_conf = self._process_frame_ocr(frame, join_char=" ")
                    
                    if text:
                        # Deduplication logic
                        similarity = SequenceMatcher(None, last_text, text).ratio()
                        if similarity < 0.9:
                            all_text.append(text)
                            all_confidences.append(frame_conf)
                            last_text = text
                    
                    frames_processed += 1
                cap.release()
                
            except Exception as e:
                logger.error(f"OCR text extraction failed: {e}")
                # frame_count = 0

            # Extract audio
            audio_results = self._extract_audio_transcription(tmp_path)
            for text, conf, _ in audio_results:
                all_text.append(text)
                all_confidences.append(conf)
                
            confidence = sum(all_confidences) / len(all_confidences) if all_confidences else 0.0
            processing_time = time.time() - start_time
            
            log_entry = {
                "source": "video_ocr_whisper",
                "success": True,
                "total_frames": frame_count,
                "frames_sampled": frames_processed,
                "text_length": sum(len(t) for t in all_text),
                "confidence": confidence,
                "processing_time": processing_time
            }
            
            return all_text, confidence, log_entry
            
        except Exception as e:
            logger.error(f"Video extraction failed: {e}")
            raise FileProcessingException("video", f"Extraction failed: {e}")
            
        finally:
            if tmp_path and os.path.exists(tmp_path):
                try:
                    os.unlink(tmp_path)
                except Exception:
                    pass

    async def extract_stream(self, content: bytes, sample_interval_seconds: float = 3.0, max_frames: int = 30) -> AsyncIterator[Tuple[str, float, Dict[str, Any]]]:
        if not content:
            raise FileProcessingException("video", "Empty video provided")
            
        tmp_path = None
        try:
            with tempfile.NamedTemporaryFile(suffix=".mp4", delete=False) as tmp:
                tmp.write(content)
                tmp_path = tmp.name
                
            try:
                target_frames, frame_count = self._detect_target_frames(tmp_path, max_frames, sample_interval_seconds)
                cap = cv2.VideoCapture(tmp_path)
                last_text = ""
                last_frame = 0
                
                for frame_idx in target_frames:
                    cap.set(cv2.CAP_PROP_POS_FRAMES, frame_idx)
                    ret, frame = cap.read()
                    if not ret:
                        continue
                        
                    text, frame_conf = self._process_frame_ocr(frame, join_char=" ")
                    
                    if text:
                        similarity = SequenceMatcher(None, last_text, text).ratio()
                        if similarity < 0.9:
                            logger.info(f"OCR similarity score of frame {frame_idx} vs frame {last_frame}: {similarity}")
                            last_text = text
                            last_frame = frame_idx
                            logger.info(f'OCR on video frame {frame_idx}: {text}')
                            yield text, frame_conf, {
                                "source": "video_ocr_frame",
                                "success": True,
                                "frame_index": frame_idx,
                                "total_frames": frame_count,
                                "text_length": len(text),
                            }
                cap.release()
            except Exception as e:
                logger.error(f"OCR text extraction failed: {e}")
                
            # Extract audio
            audio_results = self._extract_audio_transcription(tmp_path)
            for text, conf, details in audio_results:
                yield text, conf, details
                
        except Exception as e:
            logger.error(f"Video extraction failed: {e}")
            raise FileProcessingException("video", f"Extraction failed: {e}")
        finally:
            if tmp_path and os.path.exists(tmp_path):
                try:
                    os.unlink(tmp_path)
                except Exception:
                    pass
