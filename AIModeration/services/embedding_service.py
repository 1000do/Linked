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
from services.base_service import BaseService

logger = logging.getLogger(__name__)


class EmbeddingService(BaseService):
    """Generate embeddings for semantic deduplication."""
    
    # Class-level model cache (singleton pattern)
    _distilbert_model = None
    _distilbert_tokenizer = None
    _clip_model = None
    _clip_processor = None
    _models_loaded = False
    
    def __init__(self, settings: Settings = None):
        """Initialize embedding service."""
        super().__init__("EmbeddingService")
        self.settings = settings or get_settings()
        self.device = torch.device(self.settings.DEVICE)
        
        if not EmbeddingService._models_loaded:
            self._load_models()
    
    @classmethod
    def _load_models(cls):
        """Load models once (class-level cache)."""
        if cls._models_loaded:
            return
        
        logger.info("Loading embedding models...")
        
        try:
            settings = get_settings()
            device = torch.device(settings.DEVICE)
            
            # Load DistilBert for text embeddings
            logger.info(f"Loading DistilBert from {settings.DISTILBERT_MODEL_NAME}...")
            cls._distilbert_tokenizer = AutoTokenizer.from_pretrained(settings.DISTILBERT_MODEL_NAME)
            cls._distilbert_model = AutoModel.from_pretrained(settings.DISTILBERT_MODEL_NAME)
            cls._distilbert_model.to(device)
            cls._distilbert_model.eval()
            logger.info("✓ DistilBert loaded")
            
            # Load CLIP for vision embeddings
            logger.info(f"Loading CLIP from {settings.CLIP_MODEL_NAME}...")
            cls._clip_processor = CLIPProcessor.from_pretrained(settings.CLIP_MODEL_NAME)
            cls._clip_model = CLIPModel.from_pretrained(settings.CLIP_MODEL_NAME)
            cls._clip_model.to(device)
            cls._clip_model.eval()
            logger.info("✓ CLIP loaded")
            
            cls._models_loaded = True
            logger.info("✓ All embedding models loaded successfully")
            
        except Exception as e:
            logger.error(f"Failed to load embedding models: {e}")
            raise EmbeddingException("embedding_models", f"Model loading failed: {e}")
    
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
                # Tokenize
                inputs = self._distilbert_tokenizer(
                    text[:max_length * 4],  # Approximate char-to-token ratio
                    return_tensors="pt",
                    truncation=True,
                    max_length=max_length,
                    padding="max_length",
                )
                
                inputs = {k: v.to(self.device) for k, v in inputs.items()}
                
                # Get embeddings
                with torch.no_grad():
                    outputs = self._distilbert_model(**inputs)
                    # Use [CLS] token embedding (first token)
                    embedding = outputs.last_hidden_state[:, 0, :].cpu().numpy()[0]
                
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
                # Load image
                image = Image.open(io.BytesIO(image_bytes)).convert("RGB")
                
                # Process with CLIP
                inputs = self._clip_processor(images=image, return_tensors="pt", padding=True)
                inputs = {k: v.to(self.device) for k, v in inputs.items()}
                
                # Get image features
                with torch.no_grad():
                    outputs = self._clip_model.get_image_features(**inputs)
                    embedding = outputs.cpu().numpy()[0]
                
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
                            
                            # Get embedding
                            inputs = self._clip_processor(images=frame_image, return_tensors="pt", padding=True)
                            inputs = {k: v.to(self.device) for k, v in inputs.items()}
                            
                            with torch.no_grad():
                                outputs = self._clip_model.get_image_features(**inputs)
                                embedding = outputs.cpu().numpy()[0]
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
    
    async def embed_pdf_text(self, pdf_text: str) -> List[float]:
        """
        Generate embedding for PDF text.
        
        Args:
            pdf_text: Extracted text from PDF
            
        Returns:
            768-dim embedding vector
        """
        return await self.embed_text(pdf_text)
    
    async def embed_generic(self, content: bytes, material_type: str) -> List[float]:
        """
        Generate embedding for generic content type.
        
        Args:
            content: Content bytes
            material_type: Type of material (text, image, video, pdf, word)
            
        Returns:
            768-dim embedding vector
        """
        material_type = material_type.lower().strip()
        
        if material_type in ["text", "pdf", "word"]:
            # Decode text
            try:
                text = content.decode("utf-8")
                return await self.embed_text(text)
            except UnicodeDecodeError:
                # If decode fails, treat as binary embedding
                text_repr = f"Binary content {len(content)} bytes"
                return await self.embed_text(text_repr)
        
        elif material_type in ["image", "jpg", "png", "jpeg"]:
            return await self.embed_image(content)
        
        elif material_type in ["video", "mp4", "avi", "mov"]:
            return await self.embed_video_frame(content)
        
        else:
            raise EmbeddingException(material_type, f"Unsupported material type: {material_type}")
