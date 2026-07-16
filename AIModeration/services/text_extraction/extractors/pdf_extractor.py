import logging
import time
from typing import Tuple, List, Dict, Any, AsyncIterator
import fitz
from pdf2image import convert_from_bytes
import numpy as np

from core.exceptions import FileProcessingException
from .base_extractor import ITextExtractor
from providers.ml_model_provider import MLModelProvider

logger = logging.getLogger(__name__)

class PdfTextExtractor(ITextExtractor):
    """Extract text from PDF using native extraction with OCR fallback."""
    
    def __init__(self, model_provider: MLModelProvider):
        self.model_provider = model_provider

    async def extract_legacy(self, content: bytes) -> Tuple[List[str], float, Dict[str, Any]]:
        if not content:
            raise FileProcessingException("pdf", "Empty PDF provided")
        
        try:
            start_time = time.time()
            doc = fitz.open(stream=content, filetype="pdf")
            total_pages = len(doc)
            all_text = []
            has_native_text = False
            
            for idx in range(total_pages):
                page = doc.load_page(idx)
                text = page.get_text().strip()
                if text:
                    has_native_text = True
                    all_text.append(f"Page {idx+1}: {text}")
            
            doc.close()

            processing_time = time.time() - start_time

            if has_native_text:
                confidence = 1.0
                log_entry = {
                    "source": "pdf_native",
                    "success": True,
                    "pages_processed": total_pages,
                    "text_length": sum(len(t) for t in all_text),
                    "confidence": confidence,
                    "processing_time": processing_time
                }
                return all_text, confidence, log_entry

            logger.info("No native text found in PDF, falling back to OCR")
            # Fallback to OCR
            # images = convert_from_bytes(content, first_page=1, last_page=5)  # First 5 pages
            images = convert_from_bytes(content)  # All pages
            
            all_text = []
            all_confidences = []
            ocr_reader = self.model_provider.get_ocr_reader()
            
            for idx, image in enumerate(images):
                image_np = np.array(image)
                results = ocr_reader.readtext(image_np, detail=1, paragraph=False)
                page_texts = []
                for res in results:
                    if len(res) == 3:
                        _, t, prob = res
                        if t.strip():
                            page_texts.append(t)
                            all_confidences.append(float(prob))
                if page_texts:
                    all_text.append(f"Page {idx+1}: " + " ".join(page_texts))
            
            confidence = sum(all_confidences) / len(all_confidences) if all_confidences else 0.0
            processing_time = time.time() - start_time
            
            log_entry = {
                "source": "pdf_ocr",
                "success": True,
                "pages_processed": len(images),
                "text_length": sum(len(t) for t in all_text),
                "confidence": confidence,
                "processing_time": processing_time
            }
            
            return all_text, confidence, log_entry
            
        except Exception as e:
            logger.error(f"PDF extraction failed: {e}")
            raise FileProcessingException("pdf", f"Extraction failed: {e}")

    async def extract_stream(self, content: bytes) -> AsyncIterator[Tuple[str, float, Dict[str, Any]]]:
        if not content:
            raise FileProcessingException("pdf", "Empty PDF provided")
            
        try:
            doc = fitz.open(stream=content, filetype="pdf")
            total_pages = len(doc)
            has_native_text = False
            
            for idx in range(total_pages):
                page = doc.load_page(idx)
                text = page.get_text().strip()
                if text:
                    has_native_text = True
                    logger.info(f"Extracted native text from PDF page {idx + 1}: {text[:100]}...")
                    yield text, 1.0, {
                        "source": "pdf_native_page",
                        "success": True,
                        "page": idx + 1,
                        "total_pages": total_pages,
                        "text_length": len(text),
                    }
            
            doc.close()
            
            if not has_native_text:
                logger.info("No native text found in PDF, falling back to OCR")
                images = convert_from_bytes(content)
                total_pages = len(images)
                ocr_reader = self.model_provider.get_ocr_reader()
                
                for idx, image in enumerate(images):
                    image_np = np.array(image)
                    results = ocr_reader.readtext(image_np, detail=1, paragraph=False)
                    page_texts = []
                    page_confs = []
                    for res in results:
                        if len(res) == 3:
                            _, t, prob = res
                            if t.strip():
                                page_texts.append(t)
                                page_confs.append(float(prob))
                    text = " ".join(page_texts).strip()
                    confidence = sum(page_confs) / len(page_confs) if page_confs else 0.0
                    if text:
                        logger.info(f"Extracted OCR text from PDF page {idx + 1}: {text[:100]}...")
                        yield text, confidence, {
                            "source": "pdf_ocr_page",
                            "success": True,
                            "page": idx + 1,
                            "total_pages": total_pages,
                            "text_length": len(text),
                        }
        except Exception as e:
            logger.error(f"PDF extraction failed: {e}")
            raise FileProcessingException("pdf", f"Extraction failed: {e}")
