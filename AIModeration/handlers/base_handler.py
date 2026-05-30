import logging
import copy
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
                        from services.text_classifier_service import TextClassifierService
                        import torch
                        from transformers import AutoTokenizer, AutoModelForSequenceClassification

                        old_spam_model = getattr(TextClassifierService, "_spam_model", None)
                        old_spam_tok = getattr(TextClassifierService, "_spam_tokenizer", None)
                        old_toxic_model = getattr(TextClassifierService, "_toxic_model", None)
                        old_toxic_tok = getattr(TextClassifierService, "_toxic_tokenizer", None)

                        current_spam_path = getattr(TextClassifierService, "_spam_path", None)
                        current_toxic_path = getattr(TextClassifierService, "_toxic_path", None)

                        req_spam = paths[0] if len(paths) > 0 else None
                        req_toxic = paths[1] if len(paths) > 1 else None

                        if (req_spam and req_spam != current_spam_path) or (req_toxic and req_toxic != current_toxic_path):
                            self.logger.info(f"Dynamic loading requested: spam={req_spam}, toxic={req_toxic}")
                            
                            device = torch.device(getattr(TextClassifierService, "device", "cpu"))

                            if req_spam:
                                self.logger.info(f"Loading spam model from {req_spam}")
                                t = AutoTokenizer.from_pretrained(req_spam, local_files_only=True)
                                mod = AutoModelForSequenceClassification.from_pretrained(req_spam, local_files_only=True)
                                mod.to(device)
                                mod.eval()
                                TextClassifierService._spam_tokenizer = t
                                TextClassifierService._spam_model = mod
                                TextClassifierService._spam_path = req_spam

                            if req_toxic:
                                self.logger.info(f"Loading toxic model from {req_toxic}")
                                t = AutoTokenizer.from_pretrained(req_toxic, local_files_only=True)
                                mod = AutoModelForSequenceClassification.from_pretrained(req_toxic, local_files_only=True)
                                mod.to(device)
                                mod.eval()
                                TextClassifierService._toxic_tokenizer = t
                                TextClassifierService._toxic_model = mod
                                TextClassifierService._toxic_path = req_toxic

                    except Exception as e:
                        self.logger.error(f"Failed to dynamically load models from path {m.model_path}: {e}. Rolling back to default models.")
                        # Rollback
                        TextClassifierService._spam_model = old_spam_model
                        TextClassifierService._spam_tokenizer = old_spam_tok
                        TextClassifierService._toxic_model = old_toxic_model
                        TextClassifierService._toxic_tokenizer = old_toxic_tok
            processed_models.append(m)
        return processed_models
