"""Service for extracting text from various media types."""

import logging
from typing import Tuple, AsyncIterator, Dict, Any, Optional, List

from core.exceptions import FileProcessingException
from services.base_service import BaseService
from config.settings import get_settings, Settings

from providers.ml_model_provider import MLModelProvider
from .text_extraction.extractors.image_extractor import ImageTextExtractor
from .text_extraction.extractors.pdf_extractor import PdfTextExtractor
from .text_extraction.extractors.video_extractor import VideoTextExtractor
from .text_extraction.extractors.word_extractor import WordTextExtractor
from .text_extraction.extractors.excel_extractor import ExcelTextExtractor
from .text_extraction.extractors.ppt_extractor import PowerPointTextExtractor

logger = logging.getLogger(__name__)


class TextExtractionService(BaseService):
    """Orchestrator service to extract text from media files using specialized extractors."""
    
    def __init__(self, settings: Optional[Settings] = None):
        """Initialize text extraction service."""
        super().__init__("TextExtractionService")
        self.settings = settings or get_settings()
        
        # Initialize the provider
        self.model_provider = MLModelProvider(self.settings)
        
        # Register strategies (extractors)
        self.extractors = {
            "image": ImageTextExtractor(self.model_provider),
            "pdf": PdfTextExtractor(self.model_provider),
            "video": VideoTextExtractor(self.model_provider),
            "word": WordTextExtractor(),
            "excel": ExcelTextExtractor(),
            "powerpoint": PowerPointTextExtractor()
        }

    def get_file_type_for_text_extraction(self, file_extension: str) -> str:
        """
        Map file extension to appropriate generic text extraction file type.
        """
        if not file_extension:
            return "text"
        ext = file_extension.lower().replace(".", "").strip()
        if ext in ["txt", "text","csv"]:
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
    
    async def extract_generic_legacy(
        self,
        content: bytes,
        material_type: str,
    ) -> Tuple[List[str], float, dict]:
        """
        Extract text from generic material type using the appropriate strategy.
        """
        material_type = material_type.lower().strip()
        
        if material_type == "text":
            try:
                text = content.decode("utf-8")
                return [text], 1.0, {"source": "text_native", "success": True}
            except UnicodeDecodeError:
                raise FileProcessingException("text", "Invalid UTF-8 encoding")
        
        extractor = self.extractors.get(material_type)
        if not extractor:
            raise FileProcessingException(material_type, f"Unsupported file type: {material_type}")
            
        with self.time_operation(f"extract_from_{material_type}", size=len(content)):
            return await extractor.extract_legacy(content)

    async def extract_generic_stream(
        self,
        content: bytes,
        material_type: str,
    ) -> AsyncIterator[Tuple[str, float, Dict[str, Any]]]:
        """
        Extract text from generic material type as a stream of segments using the appropriate strategy.
        """
        material_type = material_type.lower().strip()
        
        if material_type == "text":
            try:
                text = content.decode("utf-8")
                logger.info(f"Extracted text from generic text: {text[:100]}...")
                yield text, 1.0, {"source": "text_native", "success": True}
            except UnicodeDecodeError:
                raise FileProcessingException("text", "Invalid UTF-8 encoding")
            return
            
        extractor = self.extractors.get(material_type)
        if not extractor:
            raise FileProcessingException(material_type, f"Unsupported file type: {material_type}")
            
        with self.time_operation(f"extract_from_{material_type}_stream", size=len(content)):
            async for result in extractor.extract_stream(content):
                yield result
