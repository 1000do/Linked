"""Service for generating embeddings using pre-trained models."""

import logging
import numpy as np
from typing import List, Optional, Tuple
import torch
from transformers import AutoTokenizer, AutoModel, CLIPProcessor, CLIPModel
from PIL import Image
import io
import cv2

from config.settings import Settings, get_settings
from core.exceptions import EmbeddingException
from core.models import EmbeddingGenerationCommand, EmbeddingGenerationResult
from services.base_service import BaseService
from services.text_extraction_service import TextExtractionService

logger = logging.getLogger(__name__)


class EmbeddingService(BaseService):
    """Generate embeddings for semantic deduplication."""
    
    def __init__(self, settings: Settings = None):
        """Initialize embedding service."""
        super().__init__("EmbeddingService")
        self.settings = settings or get_settings()
        self.device = torch.device(self.settings.DEVICE)
        self.text_extraction_service = TextExtractionService()
        
        from providers.ml_model_provider import MLModelProvider
        self.model_provider = MLModelProvider(self.settings)

    def get_file_type_for_embedding(self, file_extension: str) -> str:
        """Map file extension to allowed types in embed_generic."""
        if not file_extension:
            return "text"
        ext = file_extension.lower().replace(".", "").strip()
        if ext in ["txt", "text", "csv"]:
            return "text"
        elif ext in ["jpg", "jpeg", "png", "gif", "bmp", "tiff", "webp","image"]:
            return "image"
        elif ext in ["mp4", "avi", "mov", "mkv", "webm", "flv", "wmv", "3gp","video"]:
            return "video"
        elif ext in ["pdf"]:
            return "pdf"
        elif ext in ["docx", "doc", "odt", "rtf","word"]:
            return "word"
        elif ext in ["pptx", "ppt","powerpoint"]:
            return "powerpoint"
        elif ext in ["xlsx", "xls","excel"]:
            return "excel"
        return "text"
    
    async def embed_text(self, text: str, max_length: int = 512) -> List[float]:
        """
        Generate embedding for text using DistilBert.
        
        Args:
            text: Input text
            max_length: Max sequence length
            
        Returns:
            768-dim embedding vector
        """
        if not text or not text.strip():
            raise EmbeddingException("text", "Empty text provided")
        
        try:
            with self.time_operation("embed_text", text_length=len(text)):
                tokenizer, model = self.model_provider.get_text_embedding_model()
                # Tokenize
                inputs = tokenizer(
                    text[:max_length * 4],  # Approximate char-to-token ratio
                    return_tensors="pt",
                    truncation=True,
                    max_length=max_length,
                    padding="max_length",
                )
                
                inputs = {k: v.to(self.device) for k, v in inputs.items()}
                
                # Get embeddings
                with torch.no_grad():
                    outputs = model(**inputs)
                    # Mean pooling for sentence-transformers
                    token_embeddings = outputs.last_hidden_state
                    attention_mask = inputs['attention_mask']
                    input_mask_expanded = attention_mask.unsqueeze(-1).expand(token_embeddings.size()).float()
                    sum_embeddings = torch.sum(token_embeddings * input_mask_expanded, 1)
                    sum_mask = torch.clamp(input_mask_expanded.sum(1), min=1e-9)
                    embedding = (sum_embeddings / sum_mask).cpu().numpy()[0]
                
                return embedding.tolist()
        
        except Exception as e:
            logger.error(f"Text embedding failed: {e}")
            raise EmbeddingException("text", str(e))
    
    async def embed_image(self, image_bytes: bytes) -> List[float]:
        """
        Generate embedding for image using CLIP.
        
        Args:
            image_bytes: Image file bytes
            
        Returns:
            768-dim embedding vector
        """
        if not image_bytes:
            raise EmbeddingException("image", "Empty image provided")
        
        try:
            with self.time_operation("embed_image", image_size=len(image_bytes)):
                processor, model = self.model_provider.get_clip_model()
                # Load image
                image = Image.open(io.BytesIO(image_bytes)).convert("RGB")
                
                # Process with CLIP
                inputs = processor(images=image, return_tensors="pt", padding=True)
                inputs = {k: v.to(self.device) for k, v in inputs.items()}
                
                # Get image features
                with torch.no_grad():
                    outputs = model.get_image_features(**inputs)
                    if hasattr(outputs, "pooler_output") and outputs.pooler_output is not None:
                        embedding = outputs.pooler_output.cpu().numpy()[0]
                    elif hasattr(outputs, "last_hidden_state") and outputs.last_hidden_state is not None:
                        embedding = outputs.last_hidden_state.mean(dim=1).cpu().numpy()[0]
                    elif isinstance(outputs, torch.Tensor):
                        embedding = outputs.cpu().numpy()[0]
                    else:
                        embedding = outputs[0].cpu().numpy()[0]
                
                return embedding.tolist()
        
        except Exception as e:
            logger.error(f"Image embedding failed: {e}")
            raise EmbeddingException("image", str(e))
    
    async def embed_video_frame(self, video_bytes: bytes, sample_every_n_frames: int = 30) -> List[float]:
        """
        Generate embedding for video by sampling and averaging frames.
        
        Args:
            video_bytes: Video file bytes
            sample_every_n_frames: Sample every N frames
            
        Returns:
            768-dim embedding vector (average of sampled frames)
        """
        if not video_bytes:
            raise EmbeddingException("video", "Empty video provided")
        
        try:
            with self.time_operation("embed_video", video_size=len(video_bytes)):
                # Write bytes to temp file for OpenCV
                import tempfile
                import os
                
                with tempfile.NamedTemporaryFile(suffix=".mp4", delete=False) as tmp:
                    tmp.write(video_bytes)
                    tmp_path = tmp.name
                
                try:
                    cap = cv2.VideoCapture(tmp_path)
                    frame_count = int(cap.get(cv2.CAP_PROP_FRAME_COUNT))
                    
                    embeddings = []
                    frame_idx = 0
                    
                    while True:
                        ret, frame = cap.read()
                        if not ret:
                            break
                        
                        if frame_idx % sample_every_n_frames == 0:
                            # Convert BGR to RGB and encode as bytes
                            rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
                            frame_image = Image.fromarray(rgb_frame)
                            
                            processor, model = self.model_provider.get_clip_model()
                            
                            # Get embedding
                            inputs = processor(images=frame_image, return_tensors="pt", padding=True)
                            inputs = {k: v.to(self.device) for k, v in inputs.items()}
                            
                            with torch.no_grad():
                                outputs = model.get_image_features(**inputs)
                                if hasattr(outputs, "pooler_output") and outputs.pooler_output is not None:
                                    embedding = outputs.pooler_output.cpu().numpy()[0]
                                elif hasattr(outputs, "last_hidden_state") and outputs.last_hidden_state is not None:
                                    embedding = outputs.last_hidden_state.mean(dim=1).cpu().numpy()[0]
                                elif isinstance(outputs, torch.Tensor):
                                    embedding = outputs.cpu().numpy()[0]
                                else:
                                    embedding = outputs[0].cpu().numpy()[0]
                                embeddings.append(embedding)
                        
                        frame_idx += 1
                    
                    cap.release()
                    
                    if not embeddings:
                        raise EmbeddingException("video", "No frames extracted from video")
                    
                    # Average embeddings
                    avg_embedding = np.mean(embeddings, axis=0)
                    return avg_embedding.tolist()
                
                finally:
                    os.unlink(tmp_path)
        
        except Exception as e:
            logger.error(f"Video embedding failed: {e}")
            raise EmbeddingException("video", str(e))

    async def embed_generic(self, command: EmbeddingGenerationCommand) -> EmbeddingGenerationResult:
        """
        Generate embedding for generic content type based on EmbeddingGenerationCommand.
        """
        content = command.content
        material_type = command.material_type.lower().strip()
        if material_type in ["text"]:
            # Decode text
            try:
                text = content.decode("utf-8")
                emb = await self.embed_text(text)
            except UnicodeDecodeError:
                # If decode fails, treat as binary embedding
                text_repr = f"Binary content {len(content)} bytes"
                emb = await self.embed_text(text_repr)
                
        elif material_type in ["pdf", "word","powerpoint", "excel"]:
            embeddings_list = []
            chunk_count = 0
            
            material_type = self.text_extraction_service.get_file_type_for_text_extraction(material_type)
            async for text_chunk, conf, log in self.text_extraction_service.extract_generic_stream(content, material_type):
                if text_chunk and text_chunk.strip():
                    chunk_emb = await self.embed_text(text_chunk)
                    embeddings_list.append(chunk_emb)
                    chunk_count += 1
                
                source = log.get("source", "")
                if "ocr" in source and chunk_count >= 5:
                    logger.info(f"Early exit for OCR stream at chunk {chunk_count}")
                    break
                    
            if embeddings_list:
                avg_embedding = np.mean(embeddings_list, axis=0)
                emb = avg_embedding.tolist()
            else:
                emb = await self.embed_text(f"Empty {material_type} document")
        
        elif material_type in ["image"]:
            emb = await self.embed_image(content)
        
        elif material_type in ["video"]:
            emb = await self.embed_video_frame(content)
        
        else:
            raise EmbeddingException(material_type, f"Unsupported material type: {material_type}")
            
        return EmbeddingGenerationResult(
            material_id=command.material_id,
            model_id=command.model_id,
            embedding=emb
        )
