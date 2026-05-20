"""Service for extracting text from various media types."""

import logging
import io
import tempfile
import os
from typing import Tuple, Optional
from PIL import Image
import cv2
import pytesseract
import json

from core.exceptions import FileProcessingException
from services.base_service import BaseService

logger = logging.getLogger(__name__)


class TextExtractionService(BaseService):
    """Extract text from media files using OCR and Whisper."""
    
    def __init__(self):
        """Initialize text extraction service."""
        super().__init__("TextExtractionService")
    
    async def extract_from_image(self, image_bytes: bytes) -> Tuple[str, float, dict]:
        """
        Extract text from image using OCR.
        
        Args:
            image_bytes: Image file bytes
            
        Returns:
            (extracted_text, confidence, extraction_log)
        """
        if not image_bytes:
            raise FileProcessingException("image", "Empty image provided")
        
        try:
            with self.time_operation("extract_from_image", size=len(image_bytes)):
                # Load image
                image = Image.open(io.BytesIO(image_bytes)).convert("RGB")
                
                # Run OCR
                text = pytesseract.image_to_string(image)
                
                # Estimate confidence (pytesseract can provide per-line confidence)
                # For simplicity, use 0.8 as default if text extracted
                confidence = 0.8 if text.strip() else 0.0
                
                log_entry = {
                    "source": "image_ocr",
                    "success": True,
                    "text_length": len(text),
                    "confidence": confidence,
                }
                
                return text, confidence, log_entry
        
        except Exception as e:
            logger.error(f"Image OCR failed: {e}")
            raise FileProcessingException("image", f"OCR failed: {e}")
    
    async def extract_from_pdf(self, pdf_bytes: bytes) -> Tuple[str, float, dict]:
        """
        Extract text from PDF using OCR on rendered images.
        
        Args:
            pdf_bytes: PDF file bytes
            
        Returns:
            (extracted_text, confidence, extraction_log)
        """
        if not pdf_bytes:
            raise FileProcessingException("pdf", "Empty PDF provided")
        
        try:
            with self.time_operation("extract_from_pdf", size=len(pdf_bytes)):
                # pdf2image to convert PDF to images
                from pdf2image import convert_from_bytes
                
                images = convert_from_bytes(pdf_bytes, first_page=1, last_page=5)  # First 5 pages
                
                all_text = []
                
                for image in images:
                    text = pytesseract.image_to_string(image)
                    if text.strip():
                        all_text.append(text)
                
                combined_text = "\n".join(all_text)
                confidence = 0.75 if combined_text.strip() else 0.0
                
                log_entry = {
                    "source": "pdf_ocr",
                    "success": True,
                    "pages_processed": len(images),
                    "text_length": len(combined_text),
                    "confidence": confidence,
                }
                
                return combined_text, confidence, log_entry
        
        except Exception as e:
            logger.error(f"PDF extraction failed: {e}")
            raise FileProcessingException("pdf", f"Extraction failed: {e}")
    
    async def extract_from_video(
        self,
        video_bytes: bytes,
        sample_every_n_frames: int = 30,
    ) -> Tuple[str, float, dict]:
        """
        Extract text from video frames and audio using OCR and Whisper.
        
        Args:
            video_bytes: Video file bytes
            sample_every_n_frames: Sample every N frames for OCR
            
        Returns:
            (extracted_text, confidence, extraction_log)
        """
        if not video_bytes:
            raise FileProcessingException("video", "Empty video provided")
        
        try:
            with self.time_operation("extract_from_video", size=len(video_bytes)):
                # Write to temp file
                with tempfile.NamedTemporaryFile(suffix=".mp4", delete=False) as tmp:
                    tmp.write(video_bytes)
                    tmp_path = tmp.name
                
                all_text = []
                
                try:
                    # Extract text from frames
                    cap = cv2.VideoCapture(tmp_path)
                    frame_count = int(cap.get(cv2.CAP_PROP_FRAME_COUNT))
                    frame_idx = 0
                    
                    frames_processed = 0
                    
                    while True:
                        ret, frame = cap.read()
                        if not ret:
                            break
                        
                        if frame_idx % sample_every_n_frames == 0:
                            rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
                            image = Image.fromarray(rgb_frame)
                            
                            text = pytesseract.image_to_string(image)
                            if text.strip():
                                all_text.append(text)
                            
                            frames_processed += 1
                        
                        frame_idx += 1
                    
                    cap.release()
                    
                    # Extract audio using Whisper (if available)
                    try:
                        import whisper
                        
                        # Save audio track
                        audio_path = tmp_path.replace(".mp4", ".m4a")
                        os.system(f"ffmpeg -i {tmp_path} -q:a 9 -n {audio_path} 2>/dev/null")
                        
                        if os.path.exists(audio_path):
                            model = whisper.load_model("base")
                            result = model.transcribe(audio_path)
                            audio_text = result.get("text", "")
                            
                            if audio_text.strip():
                                all_text.append(audio_text)
                            
                            os.unlink(audio_path)
                    
                    except Exception as e:
                        logger.warning(f"Audio extraction failed (continuing with OCR): {e}")
                    
                    combined_text = "\n".join(all_text)
                    confidence = 0.7 if combined_text.strip() else 0.0
                    
                    log_entry = {
                        "source": "video_ocr_whisper",
                        "success": True,
                        "total_frames": frame_count,
                        "frames_sampled": frames_processed,
                        "text_length": len(combined_text),
                        "confidence": confidence,
                    }
                    
                    return combined_text, confidence, log_entry
                
                finally:
                    if os.path.exists(tmp_path):
                        os.unlink(tmp_path)
        
        except Exception as e:
            logger.error(f"Video extraction failed: {e}")
            raise FileProcessingException("video", f"Extraction failed: {e}")
    
    async def extract_from_word(self, docx_bytes: bytes) -> Tuple[str, float, dict]:
        """
        Extract text from Word document.
        
        Args:
            docx_bytes: DOCX file bytes
            
        Returns:
            (extracted_text, confidence, extraction_log)
        """
        if not docx_bytes:
            raise FileProcessingException("word", "Empty DOCX provided")
        
        try:
            with self.time_operation("extract_from_word", size=len(docx_bytes)):
                from docx import Document
                
                # Load from bytes
                doc = Document(io.BytesIO(docx_bytes))
                
                # Extract all text
                all_text = []
                for para in doc.paragraphs:
                    if para.text.strip():
                        all_text.append(para.text)
                
                combined_text = "\n".join(all_text)
                confidence = 0.95 if combined_text.strip() else 0.0  # High confidence for text extraction
                
                log_entry = {
                    "source": "word_native",
                    "success": True,
                    "paragraphs": len(doc.paragraphs),
                    "text_length": len(combined_text),
                    "confidence": confidence,
                }
                
                return combined_text, confidence, log_entry
        
        except Exception as e:
            logger.error(f"Word extraction failed: {e}")
            raise FileProcessingException("word", f"Extraction failed: {e}")
    
    async def extract_from_powerpoint(self, pptx_bytes: bytes) -> Tuple[str, float, dict]:
        """
        Extract text from PowerPoint presentation.
        
        Args:
            pptx_bytes: PPTX file bytes
            
        Returns:
            (extracted_text, status=PENDING_MODEL, extraction_log)
        """
        if not pptx_bytes:
            raise FileProcessingException("powerpoint", "Empty PPTX provided")
        
        try:
            with self.time_operation("extract_from_powerpoint", size=len(pptx_bytes)):
                from pptx import Presentation
                
                # Load from bytes
                prs = Presentation(io.BytesIO(pptx_bytes))
                
                # Extract text from slides
                all_text = []
                for slide_idx, slide in enumerate(prs.slides):
                    for shape in slide.shapes:
                        if hasattr(shape, "text") and shape.text.strip():
                            all_text.append(shape.text)
                
                combined_text = "\n".join(all_text)
                
                log_entry = {
                    "source": "powerpoint_native",
                    "status": "PENDING_MODEL",  # Vision model not trained yet
                    "success": True,
                    "slides": len(prs.slides),
                    "text_length": len(combined_text),
                    "confidence": 0.0,
                    "note": "Text extraction complete, but visual content requires vision model (not yet trained)",
                }
                
                return combined_text, 0.0, log_entry
        
        except Exception as e:
            logger.error(f"PowerPoint extraction failed: {e}")
            raise FileProcessingException("powerpoint", f"Extraction failed: {e}")
    
    async def extract_from_excel(self, xlsx_bytes: bytes) -> Tuple[str, float, dict]:
        """
        Extract text from Excel spreadsheet.
        
        Args:
            xlsx_bytes: XLSX file bytes
            
        Returns:
            (extracted_text, status=PENDING_MODEL, extraction_log)
        """
        if not xlsx_bytes:
            raise FileProcessingException("excel", "Empty XLSX provided")
        
        try:
            with self.time_operation("extract_from_excel", size=len(xlsx_bytes)):
                from openpyxl import load_workbook
                
                # Load from bytes
                wb = load_workbook(io.BytesIO(xlsx_bytes))
                
                # Extract all cell values
                all_text = []
                for sheet in wb.sheetnames[:5]:  # First 5 sheets
                    ws = wb[sheet]
                    for row in ws.iter_rows(values_only=True):
                        for cell in row:
                            if cell and str(cell).strip():
                                all_text.append(str(cell))
                
                combined_text = " ".join(all_text)
                
                log_entry = {
                    "source": "excel_native",
                    "status": "PENDING_MODEL",  # Visual layout analysis not available
                    "success": True,
                    "sheets_processed": min(5, len(wb.sheetnames)),
                    "text_length": len(combined_text),
                    "confidence": 0.0,
                    "note": "Cell values extracted, but visual layout analysis requires vision model (not yet trained)",
                }
                
                return combined_text, 0.0, log_entry
        
        except Exception as e:
            logger.error(f"Excel extraction failed: {e}")
            raise FileProcessingException("excel", f"Extraction failed: {e}")
    
    async def extract_generic(
        self,
        content: bytes,
        material_type: str,
    ) -> Tuple[str, float, dict]:
        """
        Extract text from generic material type.
        
        Args:
            content: File content bytes
            material_type: Type of material (text, image, video, pdf, word, pptx, xlsx)
            
        Returns:
            (extracted_text, confidence, extraction_log)
        """
        material_type = material_type.lower().strip()
        
        if material_type in ["text", "txt"]:
            try:
                text = content.decode("utf-8")
                return text, 1.0, {"source": "text_native", "success": True}
            except UnicodeDecodeError:
                raise FileProcessingException("text", "Invalid UTF-8 encoding")
        
        elif material_type in ["image", "jpg", "jpeg", "png"]:
            return await self.extract_from_image(content)
        
        elif material_type in ["pdf"]:
            return await self.extract_from_pdf(content)
        
        elif material_type in ["video", "mp4", "avi", "mov"]:
            return await self.extract_from_video(content)
        
        elif material_type in ["word", "docx", "doc"]:
            return await self.extract_from_word(content)
        
        elif material_type in ["pptx", "ppt"]:
            return await self.extract_from_powerpoint(content)
        
        elif material_type in ["xlsx", "xls"]:
            return await self.extract_from_excel(content)
        
        else:
            raise FileProcessingException(material_type, f"Unsupported file type: {material_type}")
