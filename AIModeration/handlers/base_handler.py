import logging
import copy
import os
from typing import List, Optional
from core.models import AiModelDto
from services.base_service import BaseService

logger = logging.getLogger(__name__)

class BaseHandler:
    def __init__(self, handler_name: str):
        self.handler_name = handler_name
        self.logger = logging.getLogger(f"handlers.{handler_name}")

    def process_request_models(self, models: List[AiModelDto]) -> List[AiModelDto]:
        """
        Process the models from the request.
        Compare model_paths from request with model_paths from environment/defaults.
        If different, attempts to load them dynamically and rolls back to defaults if loading fails.
        """
        self.logger.info(f"Processing {len(models)} request models for {self.handler_name}")
        processed_models = []
        for model in models:
            m = copy.deepcopy(model)
            if m.model_path:
                paths = [p.strip() for p in m.model_path.split(",") if p.strip()]
                is_classifier = m.model_type == "classifier" or (m.model_name and "classifier" in m.model_name.lower())
                if is_classifier and paths:
                    try:
                        from providers.ml_model_provider import MLModelProvider
                        from config.settings import get_settings
                        import torch
                        from transformers import AutoTokenizer, AutoModelForSequenceClassification

                        provider = MLModelProvider(get_settings())

                        old_spam_model = getattr(provider, "_spam_model", None)
                        old_spam_tok = getattr(provider, "_spam_tokenizer", None)
                        old_toxic_model = getattr(provider, "_toxic_model", None)
                        old_toxic_tok = getattr(provider, "_toxic_tokenizer", None)

                        current_spam_path = getattr(provider, "_spam_path", None)
                        current_toxic_path = getattr(provider, "_toxic_path", None)

                        req_spam = paths[0] if len(paths) > 0 else ""
                        req_spam_path = os.path.abspath(req_spam)
                        req_toxic = paths[1] if len(paths) > 1 else ""
                        req_toxic_path = os.path.abspath(req_toxic)
                        
                        

                        if (req_spam_path and req_spam_path != current_spam_path) and (req_toxic_path and req_toxic_path != current_toxic_path):
                            self.logger.info(f"Dynamic loading requested: spam={req_spam_path}, toxic={req_toxic_path}")
                            
                            device = torch.device(get_settings().DEVICE)

                            
                            self.logger.info(f"Loading spam model from {req_spam_path}")
                            spam_tokenizer = AutoTokenizer.from_pretrained(req_spam_path, local_files_only=True)
                            spam_model = AutoModelForSequenceClassification.from_pretrained(req_spam_path, local_files_only=True)
                            spam_model.to(device)
                            spam_model.eval()
                            provider._spam_tokenizer = spam_tokenizer
                            provider._spam_model = spam_model
                            provider._spam_path = req_spam_path

                            
                            self.logger.info(f"Loading toxic model from {req_toxic_path}")
                            toxic_tokenzier = AutoTokenizer.from_pretrained(req_toxic_path, local_files_only=True)
                            toxic_model = AutoModelForSequenceClassification.from_pretrained(req_toxic_path, local_files_only=True)
                            toxic_model.to(device)
                            toxic_model.eval()
                            provider._toxic_tokenizer = toxic_tokenzier
                            provider._toxic_model = toxic_model
                            provider._toxic_path = req_toxic_path
                        
                        else:
                            self.logger.info("Model paths are not provided or unchanged.. proceed to next step...")
                        

                    except Exception as e:
                        self.logger.error(f"Failed to dynamically load models from path {m.model_path}: {e}. Rolling back to default models.")
                        # Rollback
                        provider._spam_model = old_spam_model
                        provider._spam_tokenizer = old_spam_tok
                        provider._toxic_model = old_toxic_model
                        provider._toxic_tokenizer = old_toxic_tok
            processed_models.append(m)
        return processed_models
