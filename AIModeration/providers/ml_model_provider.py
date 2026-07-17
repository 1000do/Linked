import logging
import torch
import easyocr
import os
from faster_whisper import WhisperModel
from transformers import AutoTokenizer, AutoModelForSequenceClassification, AutoModel, CLIPProcessor, CLIPModel
from config.settings import Settings

logger = logging.getLogger(__name__)

class MLModelProvider:
    """Singleton provider for loading and caching heavy ML models across the application."""
    _instance = None
    
    def __new__(cls, settings: Settings = None):
        if cls._instance is None:
            cls._instance = super(MLModelProvider, cls).__new__(cls)
            cls._instance._whisper_model = None
            cls._instance._ocr_reader = None
            cls._instance.settings = settings
            
            # Caches for Text Classifier Service
            cls._instance._spam_model = None
            cls._instance._spam_tokenizer = None
            cls._instance._spam_path = None
            cls._instance._toxic_model = None
            cls._instance._toxic_tokenizer = None
            cls._instance._toxic_path = None
            
            # Caches for Embedding Service
            cls._instance._text_embedding_model = None
            cls._instance._text_embedding_tokenizer = None
            cls._instance._clip_model = None
            cls._instance._clip_processor = None
        return cls._instance
        
    def get_whisper_model(self):
        if not self._whisper_model and self.settings:
            whisper_model_name = self.settings.WHISPER_MODEL_NAME
            device = self.settings.DEVICE
            compute_type = "float16" if torch.cuda.is_available() else "int8"
            
            logger.info(f"Loading Whisper model {whisper_model_name} on {device}...")
            self._whisper_model = WhisperModel(
                whisper_model_name, 
                device=device, 
                compute_type=compute_type
            )
        return self._whisper_model
        
    def get_ocr_reader(self):
        if not self._ocr_reader:
            logger.info("Loading EasyOCR model...")
            self._ocr_reader = easyocr.Reader(['en','vi'], gpu=torch.cuda.is_available(), verbose=False)
        return self._ocr_reader

    def get_spam_model(self):
        if not self._spam_model and self.settings:
            device = torch.device(self.settings.DEVICE)
            spam_path = os.path.abspath(self.settings.SPAM_MODEL_PATH)
            logger.info(f"Loading spam model from {spam_path}...")
            self._spam_tokenizer = AutoTokenizer.from_pretrained(spam_path, local_files_only=True)
            self._spam_model = AutoModelForSequenceClassification.from_pretrained(spam_path, local_files_only=True)
            self._spam_model.to(device)
            self._spam_model.eval()
            self._spam_path = spam_path
            logger.info("✓ Spam model loaded")
        return self._spam_tokenizer, self._spam_model

    def get_toxic_model(self):
        if not self._toxic_model and self.settings:
            device = torch.device(self.settings.DEVICE)
            toxic_path = os.path.abspath(self.settings.TOXIC_MODEL_PATH)
            logger.info(f"Loading toxicity model from {toxic_path}...")
            self._toxic_tokenizer = AutoTokenizer.from_pretrained(toxic_path, local_files_only=True)
            self._toxic_model = AutoModelForSequenceClassification.from_pretrained(toxic_path, local_files_only=True)
            self._toxic_model.to(device)
            self._toxic_model.eval()
            self._toxic_path = toxic_path
            logger.info("✓ Toxicity model loaded")
        return self._toxic_tokenizer, self._toxic_model
        
    def get_text_embedding_model(self):
        if not self._text_embedding_model and self.settings:
            device = torch.device(self.settings.DEVICE)
            model_name = self.settings.TEXT_EMBEDDING_MODEL_NAME
            logger.info(f"Loading Text Embedding Model from {model_name}...")
            self._text_embedding_tokenizer = AutoTokenizer.from_pretrained(model_name)
            self._text_embedding_model = AutoModel.from_pretrained(model_name)
            self._text_embedding_model.to(device)
            self._text_embedding_model.eval()
            logger.info("✓ Text Embedding Model loaded")
        return self._text_embedding_tokenizer, self._text_embedding_model
        
    def get_clip_model(self):
        if not self._clip_model and self.settings:
            device = torch.device(self.settings.DEVICE)
            model_name = self.settings.CLIP_MODEL_NAME
            logger.info(f"Loading CLIP from {model_name}...")
            self._clip_processor = CLIPProcessor.from_pretrained(model_name)
            self._clip_model = CLIPModel.from_pretrained(model_name)
            self._clip_model.to(device)
            self._clip_model.eval()
            logger.info("✓ CLIP loaded")
        return self._clip_processor, self._clip_model
