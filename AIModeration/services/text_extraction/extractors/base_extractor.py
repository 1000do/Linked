from abc import ABC, abstractmethod
from typing import Tuple, List, Dict, Any, AsyncIterator

class ITextExtractor(ABC):
    """Abstract base class for text extraction strategies."""
    
    @abstractmethod
    async def extract_legacy(self, content: bytes) -> Tuple[List[str], float, Dict[str, Any]]:
        """
        Extract full text from the media content.
        
        Returns:
            Tuple containing:
            - List of extracted text strings
            - Confidence score (0.0 to 1.0)
            - Log entry dictionary with extraction metadata
        """
        pass
        
    @abstractmethod
    async def extract_stream(self, content: bytes) -> AsyncIterator[Tuple[str, float, Dict[str, Any]]]:
        """
        Extract text from the media content as a stream of segments.
        
        Yields:
            Tuple containing:
            - Extracted text segment string
            - Confidence score (0.0 to 1.0)
            - Log entry dictionary with extraction metadata
        """
        # Default empty async generator for subclasses that don't implement it
        if False:
            yield "", 0.0, {}
