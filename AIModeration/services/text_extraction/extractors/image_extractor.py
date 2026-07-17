import logging
import io
import time
from typing import Tuple, List, Dict, Any, AsyncIterator
from PIL import Image
import numpy as np

from core.exceptions import FileProcessingException
from .base_extractor import ITextExtractor
from providers.ml_model_provider import MLModelProvider

logger = logging.getLogger(__name__)

class ImageTextExtractor(ITextExtractor):
    """Extract text from image using OCR."""
    
    def __init__(self, model_provider: MLModelProvider):
        self.model_provider = model_provider

    async def extract_legacy(self, content: bytes) -> Tuple[List[str], float, Dict[str, Any]]:
        if not content:
            raise FileProcessingException("image", "Empty image provided")
        
        try:
            start_time = time.time()
            # Load image
            image = Image.open(io.BytesIO(content)).convert("RGB")
            
            # Run OCR
            image_np = np.array(image)
            ocr_reader = self.model_provider.get_ocr_reader()
            results = ocr_reader.readtext(image_np, detail=1, paragraph=False)
            
            texts = []
            confidences = []
            for res in results:
                if len(res) == 3:
                    _, t, prob = res
                    if t.strip():
                        texts.append(t)
                        confidences.append(float(prob))
                        
            text = texts
            confidence = sum(confidences) / len(confidences) if confidences else 0.0
            
            processing_time = time.time() - start_time
            log_entry = {
                "source": "image_ocr",
                "success": True,
                "text_length": sum(len(t) for t in text),
                "confidence": confidence,
                "processing_time": processing_time
            }
            
            logger.info(f"Extracted OCR text from image: {len(text)} segments")
            return text, confidence, log_entry
            
        except Exception as e:
            logger.error(f"Image OCR failed: {e}")
            raise FileProcessingException("image", f"OCR failed: {e}")

    async def extract_stream(self, content: bytes) -> AsyncIterator[Tuple[str, float, Dict[str, Any]]]:
        # For images, we simply yield the entire result as one segment (or we could yield segment by segment)
        # To match previous behavior, we yield the joined text or list
        # Actually, in text_extraction_service.py extract_generic_stream yielded the tuple
        text_list, conf, log = await self.extract_legacy(content)
        # Yield the full joined text or just what extract_legacy returns
        # Usually extract_stream yields text as str
        if text_list:
            joined_text = " ".join(text_list)
            yield joined_text, conf, log
