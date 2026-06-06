# ORIGINAL PLAN:

- Move exact duplication check (via MD5 hashes) to C# BE side since we already have dedicated tables to store them in the db 
- ExactDuplication checks ONLY MD5 hashes of course.title, course.description, course.what_you_will_learn, course.requirements, course.thumbnail (fileBytes)
- In BE:
	- Add to Application\DTOs:
		- ExactDuplicationCommand : CourseExt, List<CourseExt>
		- ExactDuplicationResult: int CourseId, bool IsDup, List<string> DupFields
		- CourseAIIntegrationCommand: int CourseId, int ModelId, string Role, bool IsEnabled, Dictionary<string,float> ConfigJson
		- SemanticDuplicationRequest: int CourseId, List<int> MaterialIds, float similarityScoreThreshold
		- CourseHarmfulRequest: int CourseId, float spamScoreThreshold, float toxicScoreThreshold
		- SaveCourseHashesCommand: int CourseId, title_hash, description_hash, what_you_will_learn_hash, requirements_hash, thumbnail_hash
		- CouresModerationRequest: int CourseId, int InstructorId
		- SaveCourseAiUsageLogCommand:
			- int integration_id
			- string interaction_type
			- Dictionary<string,any> input_json
			- Dictionary<string,any> output_json
			- float latency_ms
			- float token_usage
			- string error_message
			
		- StageLog:
			- int stage -- Stage number (1, 2, 3)
			- int step -- Step within stage (1, 2, 3)
			- datetime timestamp
			- string result  -- NO_MATCH, MATCH_FOUND, FLAGGED, APPROVED, etc.
			- string reason -- Explanation of result
			- List<string> flaggedFields
			- Dictionary<string,any>  details -- Additional details (matches, scores, etc.)
			- float latency_ms
			- float confidence_score -- value between 0 and 1
			
		- CourseAIModerationResult: 
			- int CourseId
			- string ModerationStatus  -- APPROVED, FLAGGED, MANUAL_AUDIT, PENDING)
			- List<string> flaggedFields
			- float overall_confidence_score  -- value between 0 and 1
			- float total_latency_ms
			- List<StageLog> stageLogs
		
	- Drop column log_status in course_ai_usage_logs, message_moderation_logs, course_review_moderation_logs, lesson_review_moderation_logs
	- Drop column ai_model_course_id in course_ai_usage_logs
	- Rename table ai_models_courses to courses_ai_integrations (rename entites, AppDBcONTEXT also)
	- Add column this column to course_ai_usage_logs table:
		- integration_id INT REFERENCES courses_ai_integrations(id) ON DELETE SET NULL
	- Add what_you_will_learn_hash and requirements_hash to course_exts table (add to entties and AppDBcONTEXT also)
	- Drop lessons_exts table ( drop from entitites and AppDBcONTEXT also)
	- Drop material_hash column in learning_materials table ( drop from entitites and AppDBcONTEXT also)

		
	- add methods string NormalizeText(string text) to ContentHashService, IContentHashService: 
		- return text after lowercase, trim spaces - replace \n and \s* with \s, etc.

	- add methods Task<string> ComputeCourseHashAsync(string text) to ContentHashService, IContentHashService: 	
		- return hashsed text (MD5)
		
	- add methods Task<string> ComputeFileHashAsync(byte[] fileBytes) to ContentHashService, IContentHashService: 	
		- return hashsed text (MD5)
		
	- add ICourseExtRepository to Domain, CourseExtRepository to Infrastructure:
	- add methods AddAsync,Update,Delete, SaveChangesAsync, GetByIdAsync, GetAllAsync to CourseExtRepository,ICourseExtRepository

	- add methods Task<void> SaveCourseHashesAsync(SaveCourseHashesCommand) to ContentHashService, IContentHashService: 	
		- Call CourseExtRepository.AddAsync()
		- Call CourseExtRepository.SaveChangesAsync()
	- add methods Task<CourseExt> GetCourseHashesAsync(int courseId) to ContentHashService, IContentHashService: 	
		- return CourseExtRepository.GetByIdAsync()
	- add methods Task<List<CourseExt>> GetAllCourseHashesAsync() to ContentHashService, IContentHashService: 	
		- return CourseExtRepository.GetAllAsync()
		

	- In CourseService.CreateCourseAsync, before return:
		- Download the course thumbnail from cloudinary 
		- Call ContentHashService.NormalizeText for course.(title, description, what_you_will_learn, requirements)
		- Call ContentHashService.ComputeCourseHashAsync for course.(title, description, what_you_will_learn, requirements)
		- Call ContentHashService.ComputeFileHashAsync for course thumbnail file
		- Create SaveCourseHashesCommand
		- Call ContentHashService.SaveCourseHashesAsync

		
		
	- add methods Task<ExactDuplicationResult> ModerationService.GetExactDuplicationResult(ExactDuplicationCommand) to IModerationService and ModerationService:	
		- Instantiate an empty List<string> dupFields
		- if (List<CourseExt>.Any(ex => ex.CourseId != CourseExt.CourseId && ex.title_hash == CourseExt.title_hash))
			-> dupFields.add('title')
		- if (List<CourseExt>.Any(ex => ex.CourseId != CourseExt.CourseId && ex.description_hash == CourseExt.description_hash))
			-> dupFields.add('description')
		- if (List<CourseExt>.Any(ex => ex.CourseId != CourseExt.CourseId && ex.what_you_will_learn_hash == CourseExt.what_you_will_learn_hash))
			-> dupFields.add('what_you_will_learn')
		- if (List<CourseExt>.Any(ex => ex.CourseId != CourseExt.CourseId && ex.requirements_hash == CourseExt.requirements_hash))
			-> dupFields.add('requirements')
		- if (List<CourseExt>.Any(ex => ex.CourseId != CourseExt.CourseId && ex.thumbnail_hash == CourseExt.thumbnail_hash))
			-> dupFields.add('thumbnail')
		- return ExactDuplicationResult (CourseExt.CourseId, dupFields.Count() > 0, dupFields)
			
	-  add methods Task<ExactDuplicationResult> CheckExactDuplication(int courseId) to IModerationService and ModerationService:	
		- CourseExt =  ContentHashService.GetCourseHashesAsync()
		- List<CourseExt> =  ContentHashService.GetAllCourseHashesAsync()
		- ExactDuplicationCommand = (CourseExt,List<CourseExt>)
		- Return ModerationService.GetExactDuplicationResult(ExactDuplicationCommand)

	-  add methods Task<int> GetAdminIdAsync() to IUserRepository and UserRepository 
		- query : select manager_id from managers where role = 'admin'
	-  add methods Task<int> GetAdminIdAsync() to IAuthService and AuthService
		- return UserRepository.GetAdminIdAsync()
	-  add methods Task<void> NotifyAdminAsync(string title, string, content, string? linkAction) to IModerationService and ModerationService:
		- adminId  = AuthService.GetAdminIdAsync()
		- Call NotificationService.SendNotificationAsync()
		
	-  add MaterialEmbeddingRepository to Infrastructure, IMaterialEmbeddingRepository to Domain
	-  add methods AddAsync,Update,Delete, SaveChangesAsync,GetAllAsync to MaterialEmbeddingRepository, IMaterialEmbeddingRepository:
		- query: select * from material_embeddings
	-  add methods Task<List<MaterialEmbedding>> GetAllMaterialEmbeddingsAsync() to ILessonService and LessonService:
		- Return MaterialEmbeddingRepository.GetAllAsync()

	-  add methods Task<void> PrepareMaterialEmbeddingsAsync() to IModerationService and ModerationService:
		- Call LessonService.GetAllMaterialEmbeddingsAsync to get all of the existing MaterialEmbeddings in db
		- For each MaterialEmbedding, Call RedisService.SetCacheAsync() with the following details:
			- key : "material_embedding:{embedding_id}"
			- value: MaterialEmbedding
			- expiry: 3 hours
			
	-  add ISystemConfigRepository to Domain, SystemConfigRepository to Infrastructure

	-  add methods Task<Dictionary<string,any>> GetConfigByKeyAsync(string key) to ISystemConfigRepository and SystemConfigRepository:
		- query: select config_value from system_configs where config_key = ?

			
	-  add methods Task<Dictionary<string,any>> GetScoreThresholdConfigAsync(string key) to IAiModerationService and AiModeration:
		- Return SystemConfigRepository.GetConfigByKeyAsync() to get score thresholds
		
	-  add IAiModelRepository to Domain, AiModelRepository to Infrastructure
	-  add methods Task <List<int>> GetModelIdsByType(string modelType) to IAiModelRepository, AiModelRepository:
		- query: select model_id from ai_models where model_status = 'active' and model_type = ?
	-  add methods Task<List<int>> GetModelIdsByType(string modelType) to IAiModerationService and AiModeration:
		- Return AiModelRepository.GetModelIdsByType()
		
	-  add ICourseAIIntegrationRepository to Domain, CourseAIIntegrationRepository to Infrastructure
	-  add methods AddAsync, SaveChangesAsync to ICourseAIIntegrationRepository, CourseAIIntegrationRepository
		
	-  add Task<bool> CourseService.IntegrateAItoCourseAsync(CourseAIIntegrationCommand) to ICourseService, CourseService: 
		- Create CourseAiIntegration object from CourseAIIntegrationCommand
		- valueTaskAdd = CourseAIIntegrationRepository.AddAsync()
		- valueTaskSave = CourseAIIntegrationRepository.SaveChangesAsync()
		- return valueTaskAdd.IsCompletedSuccessfully && valueTaskSave.IsCompletedSuccessfully
		
		

	-  add methods Task<void> AssignAIModeratorsToCourseAsync(int courseId) to IModerationService and ModerationService:
		- Call AiModerationService.GetScoreThresholdConfigAsync() to get score thresholds
		- Call AiModerationService.GetModelIdsByType() to get model ids
		- For each model id:
			- Create a CourseAIIntegrationCommand
			- Call CourseService.IntegrateAIToCourseAsync to add entry to courses_ai_integrations table
		

	-  add methods Task<bool> PrepareForCourseAIModeration(int courseId) to IModerationService and ModerationService:
		- assignSuccess = ModerationService.AssignAIModeratorsToCourseAsync
		- if (assignSuccess)
			- Call CourseService.GetCourseWithDetailsAsync to get course details along with nested lessons and materials (this would also cache the course in redis)
			- Call ModerationService.PrepareMaterialEmbeddingsAsync()
		- otherwise
			- throw exception

	-  add methods Task<CourseAIModerationResult> ModerateCourseFullPipelineAsync(SemanticDuplicationRequest,CourseHarmfulRequest) to IAiModerationService and AiModerationService:
		- Request the FastAPI for full pipeline moderation (/moderation/full-pipeline)

	-  add methods Task<?> ResolveCourseAIModerationResult(?) to IModerationService and ModerationService:
		- Query redis with material_ids to get the newly generated embeddings and save to materials_embeddings table if any:
			- (working on it)
		- need to update course,lesson,material status and moderation_feedback text according to the AI moderation result, then notify instructor
			- (working on it)
		
	-  add CourseAiUsageLogRepository to Infrastructure, ICourseAiUsageLogRepository to Domain
	-  add AddAsync, SaveChangesAsync to CourseAiUsageLogRepository, ICourseAiUsageLogRepository
	-  add methods Task<void>	SaveCourseAiUsageLog(SaveCourseAiUsageLogCommand) to IAiModerationService and AiModerationService:
		- call CourseAiUsageLogRepository.AddAsync()
		- call CourseAiUsageLogRepository.SaveChangesAsync()
		

	-  add methods Task<void>	LogCourseAiModerationResult(CourseAIModerationResult) to IModerationService and ModerationService:
		- create SaveCourseAiUsageLogCommand
		- call AiModerationService.SaveCourseAiUsageLog()
		
	-  add methods Task<CourseAIModerationResult>	HandleCourseModerationWithAIAsync(CouresModerationRequest) to IModerationService and ModerationService:
		- Call AiModerationService.HealthCheck():
		- Unhealthy:
			- Call ModerationService.NotifyAdminAsync() to notify admin for course manual review
		- Healthy:
			- Call ModerationService.PrepareForCourseAIModeration()
			
			- CourseAIModerationResult = AiModerationService.ModerateCourseFullPipelineAsync():
			- if(CourseAIModerationResult.ModerationStatus == MANUAL_AUDIT)
				- Call ModerationService.NotifyAdminAsync() to notify admin for course manual review
			- otherwise:
				- Call ModerationService.ResolveCourseAIModerationResult()
			- Call ModerationService.LogCourseAiModerationResult()
			
			
		- HTTPException:
			if(statusCode in (500,422)) 
				- Call ModerationService.NotifyAdminAsync() to notify admin for course manual review
			otherwise:
				- throw Exception
		- Exception:
			- log
			- throw
		

	- add methods Task<void> ModerationService.HandleCourseModerationAsync(CouresModerationRequest) to IModerationService and ModerationService
	- In HandleCourseModerationAsync:
		- Call CourseService.UpdateCourseStatusAsync to update course status to "Pending"
		- ExactDuplicationResult = ModerationService.CheckExactDuplication()
		- if (ExactDuplicationResult.IsDup)
			- Call ModerationService.RejectCourseDetailedAsync()
		- otherwise
			- Call ModerationService.HandleCourseModerationWithAIAsync()
		- Exception:
			- log
			- throw
					
					
		
	- Create 2 api endpoints (ModerateCourse) in CourseMarketplaceFE\Controllers\InstructorCourseController.cs and CourseMarketplaceBE\Presentation\Controllers\CourseController.cs for moderation specifically (Decoup the moderation trigger from UpdateCourseStatus endpoints)
	- In FE ModerateCourse:
		- Call BE ModerateCourse
	- In BE ModerateCourse:
		- Call GetInstructorId()
		- Call ModerationService.HandleCourseModerationAsync()
		- Return status code with exception handling just like UpdateCourseStatus endpoint
	- User click the button "Submit for Review" in Editor.cshtml, this would call the newly created api endpoints ModerateCourse.

# IMPROVEMENT

## Modified AI moderation pipeline:
- Stage 1: SemanticDeDuplication only (Let c# handle the exact hash dedup)
  - Step 1: 
    - Validate semantic duplication via learning_material embeddings + cosine similarity
      - For video, image: CLIP
      - For PDF, Word: DistilBert
      - For excel, ppt slides: ???
    - Table: learning_materials, material_embeddings

- Stage 2: Toxicity + Spam = Harmful (narrow down the scope, focus on classification with raw_text and extracted_text only)
  - Step 1: 
    - Check course.title, course.description, course.what_you_will_learn, course.requirements, lesson.title, learning_material.title, learning_material.description using my finetuned DistilBert (Ensemble of toxicity dection and spam detection)
    - If there is a detection (FLAGGED), send a response back to C# for immediate rejection 
  - Step 2: 
    - Download course.thumbnail and learning_materials using course.thumbnail_url and learning_material.material_url from cloudinary
    - For on-image or on-frame text, preprocess course.thumbnail and learning_materials (video, image, pdf, word, etc.) with FastOCR, Whisper to get processed_text 
    - Use my finetuned DistilBert to run inference on processed_text and raw_text if any
    - If there is a detection (FLAGGED), send a response back to C# for immediate rejection 
  
- At any of these steps above, the response to C# contains the whole stacked logs from the entry point to the exit point


		
	
## In CourseMarketplaceBE:
- Update Entities, AppDbContext, DTOs to match with 5.sql
- Add MaterialEmbeddingResponse to Application\DTOs:
  - public int EmbeddingId { get; set; }
  - public int? MaterialId { get; set; }
  - public string? Embedding { get; set; }
- Analyze potential erros if we change the references of Entities\MaterialEmbedding in Application\Services and Infrastructure\Services to MaterialEmbeddingResponse
- Execute the change and fix the erros if any

- Rename AiModelResponse to AiModelDto

- modify SemanticDuplicationRequest and HarmfulCourseRequest in Application\DTOs:
  add List<AiModelDto> Models (with json property "models")
      
- modify AssignAIModeratorsToCourseAsync:
  - args: int courseId, List<AiModelDto> models
  - return: unchanged
  - instead of GetModelsByTypeAsync, use the models from args
  - Value of Role for CourseAIIntegrationCommand should be changed to this format: $"{ModelType}_{ProcessType}" (e.g embedding_generator_media)

- add method GetByModelPathAsync to AiModelReposiotry to retrieve AiModelDto using ModelPath





- on course moderation, in ModerationService.PrepareForCourseAIModeration:
  - query system_configs table for config_key "course_harmful_text_classifier", "course_text_embedding_generator", "course_media_embedding_generator" (SystemConfigRepository.GetValueAsync), this should return config_value as model_path for each config_key
  - query ai_models with model_path (AiModelReposiotry.GetByModelPathAsync) for each model_path -> get collection of models
  - query course_ai_integrations with course_id (CourseAiIntegrationRepository.GetByCourseIdAsync)
  - if not exists -> insert to course_ai_integrations (coure_id, model_id, other values, etc. ):
    AssignAIModeratorsToCourseAsync(course_id, models)
  - otherwise (the following is just pseudo code that need to be translated to C#):
```bash
update_count = 0			
      
for integration in course_ai_integrations:
  is_updated = False
  role = integration.role.lower()
  integrated_model_id = integration.model_id
  
  for model in models:
    model_id = model.model_id
    model_type = model.model_type.lower()
    process_type = model.process_type.lower()
  
    if process_type in role and model_type in role:
      if model_id != integrated_model_id:
        update_count++
        is_updated = True
        integrated_model_id = model_id
      
      break
      
  if is_updated:
    Update the integration in DbSet
    
if update_count > 0:
  SaveChangesAsync()
```

## In FastAPI:
- In core\models:
  - rename and modify:
    - DuplicationRequest to match with C# SemanticDuplicationRequest
    - ToxicityRequest to match with C# CourseHarmfulRequest
  - rename: 
    - ModerationResponse to CourseModerationResponse
  - modify:
    - StageLog to match with C# StageLog
  
- In FastAPI, add a "handlers" layer between "services" and "routes"
- routes -> handlers -> services
- In handlers layer, add files base_handler, duplication_handler, harmful_handler
- Rename toxicity_service to harmful_service
- Rename check_text_toxicity to check_text_harmful
- Rename check_media_text_toxicity to check_media_text_harmful
- Drop classify_text, _sliding_window_aggregate, _aggregate_predictions from text_classifier_service

- In duplication_service:
  - remove check_exact_duplication
  - move orchestrate_stage1 to handlers layer with some changes

- In harmful_service:
  - remove check_visual_toxicity
  - move orchestrate_stage2 to handlers layer with some changes 
  
- Move method cosine_similarity from cache_repository to duplication_service
- Remove method find_similar_embeddings from cache_repository

  
- In services, add redis_service
- Add DTOs to core\models with fields in snake_case:
  - AiModelDto:
    model_id : int,
    model_name : str,
    model_type : str,
    model_provider : str ,
    model_version : str,
    model_status : str,
    description : str,
    model_created_at : datetime,
    model_updated_at: datetime,
    model_path: str,
    process_type: str
    
  - EmbeddingGenerationCommand:
    material_id: int,
    content: bytes,
    model_id: int,
    material_type: str
    
  - EmbeddingGenerationResult:
    material_id: int,
    model_id: int,
    embedding: List[float]
    
  - MaterialEmbeddingDto:
    embedding_id: int,
    material_id: int,
    embedding: List[float]
    
  - MaterialMetadataDto mirrors CourseMarketplaceBE\Domain\Entities\MaterialMetadata 
  - MaterialDto mirrors CourseMarketplaceBE\Application\DTOs\MaterialResponse
  - LessonDto mirrors CourseMarketplaceBE\Application\DTOs\LessonResponse
  - CourseDto mirrors CourseMarketplaceBE\Application\DTOs\CourseResponse
  - CourseDetailDto mirrors CourseMarketplaceBE\Application\DTOs\CourseDetailResponse
  
- modify SemanticDuplicationRequest and HarmfulCourseRequest in core\models:
  add List[AiModelDto] models
  

- Add prefix constants to cache_repository:
  - AI_MODEL_PREFIX = 'ai_models'
  - EMBEDDINGS_INITIALIZED = 'material_embedding:initialized'
  
- Add method get_course_details(int course_id) to redis_service:
  - course_data = cache_repository.get_course_data
  - map course_data to CourseDetailDto
  - return CourseDetailDto
  
- Add method get_ai_models(string key_suffix) to cache_repository:
  - cache_key = f'{AI_MODEL_PREFIX}:{key_suffix}'
  - query redis cache with cache_key 
  - return the query result
- Add method get_ai_models(string key_suffix) to redis_service:
  - models = cache_repository.get_ai_models
  - map models to List[AiModelDto]
  - return List[AiModelDto]
  

  
- Add method set_material_embedding(MaterialEmbeddingDto dto, int ttl) to redis_service:
  - Extract material_id and embedding from dto
  - return cache_repository.set_material_embedding(material_id,embedding,ttl)
  
  

  
- Add method get_existing_embeddings(string key_prefix = 'material_embedding:') to cache_repository:
  - query redis cache for all matches by scanning the key_prefix until cursor == 0
  - exclude the result where cache key == 'material_embedding:initialized'
  - return the final result
  
- Add method get_all_existing_embeddings() to redis_service():
  - embeddings = cache_repository.get_existing_embeddings()
  - map embeddings to List[MaterialEmbeddingDto]
  - return List[MaterialEmbeddingDto]

- In base_handler, init it similar to base_service with logger and handler_name
- In duplication_handler: 
  - extends base_handler
  - move orchestrate_stage1 from duplication_service to this
- In harmful_handler: 
  - extends base_handler
  - move orchestrate_stage2 from harmful_service to this
  
- Modify embeddings_service.embed_generic:
  - args: EmbeddingGenerationCommand
  - return : EmbeddingGenerationResult
  - detailed implementation of the method stay the same
  
- Add method process_request_models to base_handler:
  - args: List[AiModelDto]
  - return: List[AiModelDto]
  - extract the data of models from the request DTO
  - because model_path is comma-separated, we need to process_text on model_path (separates the paths by ",")
  - compare model_paths from request with model_paths from env
  - if different:
    - store the default models in temporary variables
    - load model with new model_paths from request
    - if loading failed, fall back to default models (stored in temporary vars, no need to reload)
  - return data of loaded models as List[AiModelDto]
  
  
- Add method similarity_search to duplication_service:
  - arguments: MaterialEmbeddingDto new_embedding, List[MaterialEmbeddingDto] existing_embeddings, float threshold
  - return: Dict[str,Any]
  - for embedding_dto in existing_embeddings:
    - similiarity = cosine_similarity(new_embedding.embedding, embedding_dto.embedding)
    - if similarity >= threshold:
      return {'material_id': embedding_dto.material_id,
          'similarity_score': float(similarity)
          }
  
- Modify duplication_service.check_semantic_duplication:
  - arguments : List[MaterialEmbeddingDto] new_embeddings , List[MaterialEmbeddingDto] existing_embeddings, int model_id, float threshold
  - return: bool is_duplicate, List[int] matched_material_ids, List[float] similarity_scores, List[StageLog] stage_logs
  - if new_embeddings is falsy or empty -> skip due to no embeddings provided (like the current implementation)
  - otherwise:
    - matched_material_ids = []
          - similarity_scores = []
    - For each new_embedding in new_embeddings:
      - similar : Dict[str,Any] = similarity_search(new_embedding, existing_embeddings, threshold)
      - if similar:
        - matched_material_ids.append(similar['material_id'])
        - similarity_scores.append(similar['similarity_score'])
    - is_duplicate = len(matched_material_ids) > 0
    - confidence_score = average of similarity_scores 
    - ...build_stage_log, exception, return... (like the current implementation)



- Modify orchestrate_stage1:
  - arguments: SemanticDuplicationRequest request
  - return: CourseModerationResponse
  - course_id = request.course_id
  - material_ids = request.material_ids
  - threshold = request.similarity_score_threshold
  - generators : List[AiModelDto] = process_request_models(request.models)
  
  - all_stage_logs : List[StageLog] = []
  
  
  - existing_embeddings : List[MaterialEmbeddingDto] = redis_service.get_all_existing_embeddings() (get the existing embeddings which were cached from C# BE side)
  - course : CourseDetailDto = redis_service.get_course_details(course_id)  (get course data which was cached from C# BE side)
  - material_id_set = set(material_ids)
  - matches : List[MaterialDto] = [
    material
    for lesson in course.lessons
    for material in lesson.materials
    if material.material_id in material_id_set
  ]
  - new_embeddings : List[MaterialEmbeddingDto] = []
  
  - For each MaterialDto in matches:
    - download the file bytes from cloudinary using MaterialDto.material_url
    - extract file_extension from MaterialDto.MaterialMetadataDto
    - map file_extension to AiModelDto.process_type to get the appropriate AiModelDto.model_id from generators
    - map file_extension to EmbeddingGenerationCommand.material_type
    - create EmbeddingGenerationCommand
    - emb_result : EmbeddingGenerationResult = embedding_service.embed_generic(EmbeddingGenerationCommand)
    - new_embedding = MaterialEmbeddingDto(
      embedding_id = None,
      material_id = emb_result.material_id,
      embedding = emb_result.embedding
    )
    - new_embeddings.append(new_embedding)
    
  - is_duplicate, matched_material_ids, similarity_scores, stage_logs =  duplication_service.check_semantic_duplication(new_embeddings,existing_embeddings, emb_result.model_id, threshold)
  - all_stage_logs.extend(stage_logs)
  - overall_confidence_score = average of similarity_scores
  - if is_duplicate:
    - return CourseModerationResponse() with moderation_status = ModerationStatus.FLAGGED.value

  - otherwise:
    - For each new_embedding in new_embeddings:
      - call redis_service.set_material_embedding(new_embedding, ttl = None) to cache the newly generated embeddings to redis
    - return CourseModerationResponse() with moderation_status = ModerationStatus.APPROVED.value
    

- Add to text_classifer_service, modify the return type to match with other implementation, additionally include latency, logger where neccessary:


```python
def classify_text(self, text, spam_threshold = 0.85, toxic_threshold = 0.85, window_size = 128, stride = 64):
    
  
  res : list = self.robust_sliding_window(
                  text, 
                  window_size=window_size,
                  stride=stride)
                  
    
                  
  
  
  return self.aggregation_logic(res, spam_threshold, toxic_threshold)
``` 
```python
def robust_sliding_window(self, text, window_size=128, stride=64):
  spam_model = self.spam_model
  toxic_model = self.toxic_model
  spam_model.eval()
  toxic_model.eval()
  tokenizer = self.spam_tokenizer
  tokens = tokenizer.encode(text, add_special_tokens=False)
  length = len(tokens)
  # If text is short, just do a normal pass
  # Minus 2 special tokens (cls, sep)
  if length <= window_size - 2:
    return [self.check_course_description(text)]

  chunk_results = []

  for i in range(0, length, stride):
    chunk = tokens[i : i + window_size]
    encoded_chunk = tokenizer(
      tokenizer.convert_ids_to_tokens(chunk), # Convert back to tokens briefly
      is_split_into_words=True,              # Tell it not to re-split
      truncation=True,
      padding='max_length',     # This fills the "decimal" gap with 0s
      max_length=window_size, 
      add_special_tokens=True,
      return_tensors="pt"
    ).to(DEVICE)
    
    
    with torch.no_grad():
      spam_outputs = spam_model(**encoded_chunk)
      toxic_outputs = toxic_model(**encoded_chunk)
      
      spam_probs = F.softmax(spam_outputs.logits, dim=-1)
      toxic_probs = F.softmax(toxic_outputs.logits, dim=-1)
    
    spam_conf, spam_pred = torch.max(spam_probs, dim=1)
    toxic_conf, toxic_pred = torch.max(toxic_probs, dim=1)
    
    chunk_results.append({
      "text" : tokenizer.decode(chunk),
      "spam_score": spam_conf.item(),
      "spam_label": spam_model.config.id2label[spam_pred.item()], # Dynamic label extraction
      "toxic_score": toxic_conf.item(),
      "toxic_label": toxic_model.config.id2label[toxic_pred.item()], # Dynamic label extraction
      
    })
    
    if i + window_size >= length: break

  
  
  return chunk_results
```
```python  
def check_course_description(self, text):
  spam_model = self.spam_model
  toxic_model = self.toxic_model
  tokenizer = self.spam_tokenizer
  spam_model.eval()
  toxic_model.eval()
  inputs = tokenizer(text, return_tensors="pt", truncation=True, padding='max_length',max_length=128).to(DEVICE)
  
  with torch.no_grad():
    spam_outputs = spam_model(**inputs)
    toxic_outputs = toxic_model(**inputs)
    # Convert raw logits to probabilities (0.0 to 1.0)
    spam_probs = F.softmax(spam_outputs.logits, dim=-1)
    toxic_probs = F.softmax(toxic_outputs.logits, dim=-1)
  
  # Get the max probability and the predicted class
  spam_conf_score, spam_prediction = torch.max(spam_probs, dim=1)
  spam_conf_score = spam_conf_score.item()
  spam_prediction = spam_prediction.item()    
  
  
  toxic_conf_score, toxic_prediction = torch.max(toxic_probs, dim=1)
  toxic_conf_score = toxic_conf_score.item()
  toxic_prediction = toxic_prediction.item()
  
  
  # DYNAMIC LABEL: Uses id2label mapping 
  spam_label = spam_model.config.id2label[spam_prediction]
  toxic_label = toxic_model.config.id2label[toxic_prediction]
  
    

  # Base response
  res = {
  "text": text, 
  "spam_score": spam_conf_score,
  "spam_label": spam_label,
  "toxic_score:" toxic_conf_score,
  "toxic_label": toxic_label
  }
    
    
  return res
```
```python 
def aggregation_logic(self, chunk_results, spam_threshold = 0.85, toxic_threshold = 0.85):
  
  # 1. Identify High-Confidence Threats (The most dangerous)
  high_conf_threats = [
    r 
    for r in chunk_results
    if (r["spam_label"] != "SAFE" and r['spam_score'] >= spam_threshold)
    or (r["toxic_label"] != 'SAFE' and r['toxic_score'] >= toxic_threshold)
  ]
  
  if high_conf_threats:
    
    # If multiple, take the one the model is MOST sure about
    worst_spam = max(high_conf_threats, key=lambda x: x["spam_score"])
    worst_toxic = max(high_conf_threats, key=lambda x: x["toxic_score"])
    
    
    if worst_spam['spam_score'] > worst_toxic['toxic_score']:
      text = worst_spam['text']
      score = worst_spam['spam_score']
      raw_label = worst_spam['spam_label']   
    else: 
      text = worst_toxic['text']
      score = worst_toxic['toxic_score']
      raw_label = worst_toxic['toxic_label']
      
    return {
      'text': text,
      'action': 'FLAGGED',
      'score': score,
      'raw_label': raw_label
    }
      
    

  # 2. Identify Low-Confidence Threats (Model is suspicious)
  low_conf_threats = [
    r 
    for r in chunk_results
    if (r["spam_label"] != "SAFE" and r['spam_score'] < spam_threshold)
    or (r["toxic_label"] != 'SAFE' and r['toxic_score'] < toxic_threshold)
  ]
  
  if low_conf_threats:
    # Audit this immediately. Model thinks it's a scam but needs a human.
    most_suspicious_spam = max(low_conf_threats, key=lambda x: x["spam_score"])
    most_suspicious_toxic = max(low_conf_threats, key=lambda x: x["toxic_score"])
    
    if most_suspicious_spam['spam_score'] > most_suspicious_toxic['toxic_score']:
      text = most_suspicious_spam['text']
      score = most_suspicious_spam['spam_score']
      raw_label = most_suspicious_spam['spam_label']
      
    else:
      text = most_suspicious_toxic['text']
      score = most_suspicious_toxic['toxic_score']
      raw_label = most_suspicious_toxic['toxic_label']
    
    return {
      "text": text,
      "action": "MANUAL_AUDIT", 
      "reason": "Probable Threat (Low Confidence)", 
      "score": score, 
      "raw_label": raw_label
    }

  # 3. Identify Low-Confidence Safes (Model is confused)
  # This addresses your 0.6, 0.7, 0.8 example
  
  low_conf_safes = [
    r 
    for r in chunk_results
    if (r["spam_label"] == "SAFE" and r['spam_score'] < spam_threshold)
    or (r["toxic_label"] == 'SAFE' and r['toxic_score'] < toxic_threshold)
  ]
  if low_conf_safes:
    # Audit this because the model isn't sure it's safe.
    # We take the one closest to 0.5 (the most confused)
    most_confused_spam = min(low_conf_safes, key=lambda x: x["spam_score"])
    most_confused_toxic = min(low_conf_safes, key=lambda x: x["toxic_score"])
    
    if most_confused_spam['spam_score'] < most_confused_toxic['toxic_score']:
      text = most_confused_spam['text']
      score = most_confused_spam['spam_score']
      raw_label = most_confused_spam['spam_label']
    else:
      text = most_confused_spam['text']
      score = most_confused_spam['toxic_score']
      raw_label = most_confused_spam['toxic_label']
      
    return {
      "text": text,
      "action": "MANUAL_AUDIT", 
      "reason": "Ambiguous Content (Low Confidence Safe)", 
      "score": score, 
      "raw_label": raw_label
    }

  # 4. If we got here, every single chunk is SAFE and > threshold.
  # Return the average score of the safest course.
  avg_score = sum(r["spam_score"] + r['toxic_score'] for r in chunk_results) / (2 * len(chunk_results))
  return {"action": "APPROVED", "score": avg_score, "raw_label": "SAFE"}
```
- Modify harmful_service.check_text_harmful:
  - arguments: CourseDetailDTO course, int model_id, float spam_threshold, float toxic_threshold
  - return: bool is_flagged, List[str] flagged_fields, List[StageLog] stage_logs
  - step = 1
  - flagged_fields : List[str] = []
      - aggregate_scores : List[float] = []
  - overall_details : Dict[str,Any] = {}
  - if course.title:
    - result, conf, details = await self.text_classifier.classify_text(course.title, spam_threshold, toxic_threshold)
    - overall_details['course.title'] = details
    - if result == ModerationStatus.FLAGGED.value:
      flagged_fields.append("course.title")
      aggregate_scores.append(conf)
  - do the same thing for course.description, course.what_you_will_learn, course.requirements; lesson.title for all lessons in the course; material.title, material.description for all materials in every lesson
      - flagged_count = len(flagged_fields) 
  - is_flagged = flagged_count > 0
  - confidence_score = average of aggregate_scores
  - reason =  f"Harmful content detected in fields: {', '.join(flagged_fields)}"
                  if is_flagged
                  else "No harmful content detected in text fields"
  - overall_details['flagged_count'] = flagged_count
  - result= ModerationStatus.FLAGGED.value if is_flagged else ModerationStatus.APPROVED.value
              
  - stage_logs.append(build_stage_log with the above info)
  - return is_flagged, flagged_fields, stage_logs
  
  
  
- Modify harmful_service.check_media_text_harmful:
  - arguments: Dict[str,Any] candidates, int model_id, float spam_threshold, float toxic_threshold
  - return: bool is_flagged, List[str] flagged_fields, List[StageLog] stage_logs
  - step = 2
      - stage_logs = []
  - if no candidates provided -> skip (similiar to the current implementation)
  - otherwise:
    - flagged_content = []
          - aggregate_scores : List[float] = []
    - overall_details : Dict[str,Any] = {}
          - candidates_checked = 0
          - candidates_pending = 0
    
    - for key,value in candidates.items():
      - candidate_alias = key
      - file_type = value['file_type']
      - file_bytes = value['file_bytes']
      
      - if no file_bytes -> skip due to no file bytes provided
      
      - if file_type in ["pptx", "ppt", "xlsx", "xls"] -> skip due to pending model
      
      
      - extracted_text,_,_ = text_extraction_service.extract_generic(
        content=file_bytes,
        material_type=file_type,
      )
      
      - if not extracted_text or not extracted_text.strip() -> skip due to no text extracted
              
      - candidates_checked +=1
      
      - result, conf, details = text_classifier_service.classify_text(extracted_text,spam_threshold,toxic_threshold)
      - overall_details[candidate_alias] = details
      - if result == ModerationStatus.FLAGGED.value:
        - flagged_content.append(candidate_alias)
        - aggregate_scores.append(conf)
        
    - flagged_count = len(flagged_content)
    - is_flagged = flagged_count > 0
    - confidence_score = average of aggregate_scores
    - reason =  f"Harmful content detected in: {', '.join(flagged_fields)}"
          if is_flagged
          else "No harmful content detected in course media and resources"
    - overall_details['flagged_count'] = flagged_count
    - result= ModerationStatus.FLAGGED.value if is_flagged else ModerationStatus.APPROVED.value
              
    - stage_logs.append(build_stage_log with the above info)
    - return is_flagged, flagged_fields, stage_logs
    
    
      
      

- Add method get_file_type_for_text_extraction to text_extraction_service:
  - agrs: str file_extension
  - return: str file_type
  - Map file_extension to appropriate file_type in text_extraction_service.extract_generic

- Modify orchestrate_stage2:
  - arguments: CourseHarmfulRequest request
  - return: CourseModerationResponse
  - spam_threshold = request.spam_score_threshold
  - toxic_threshold = request.toxic_score_threshold
  - classifiers : List[AiModelDto] = process_request_models(request.models)
  - all_stage_logs : List[StageLog] = []
  
  
  - for model in classifiers:
    model_id = model.model_id if 'harmful_text_classifier' in model.model_name.lower()
    break
  - course : CourseDetailDto = redis_service.get_course_details(course_id)  (get course data which was cached from C# BE side)
  - text_flagged, flagged_fields, step1_logs = harmful_service.check_text_harmful(course, model_id , spam_threshold, toxic_threshold)
    
  - all_stage_logs.extend(step1_logs)
  - return immediately if text_flagged 
  - step2_candidates : Dict[str,Any] = {}
  - for course thumbnail:
    - thumnail_bytes = download from cloudinary using course.thumbnail_url
    - thumbnail_type = "image"
    - step2_candidates['course.thumbnail'] = {
      'file_type' : thumbnail_type,
      'file_bytes': thumnail_bytes
    }
  - materials : List[MaterialDto] = [
    material 
    for lesson in course.lessons
    for material in lesson.materials
  ]

  - for material in materials:
    - material_bytes = download from cloudinary using material.material_url
    - metadata = material.MaterialMetadataDto
    - material_type = text_extraction_service.get_file_type_for_text_extraction(metadata.file_extension)
    - step2_candidates[f"material_{material.material_id}"] = {
      'file_type' : material_type,
      'file_bytes': material_bytes
    }


  - perform harmful_service.check_media_text_harmful on downloaded material files and course thumbnail
  - store the current stage_log
  - return immediately if any media_flagged
  - the rest of the method stay the same
  


UNIQUE CONSTRAINT on course_exts hash columns
but we need to find a way to notify the instructor which fields violate the constraint
	
	
tach may cai integration ra service rieng	
xong nho tach may cai ham` course hash ra file rieng (Service)	
tach notifyadmin qua notification service
sua ham` flag detail de no thong bao va ghi log chuan? hon
tach may cai dto cua AI ra  rieng
in create course: lockactive -> throw ????	
	
		 
docker-compose
Editor
vscode/setting json


add checklist auto reject (khỏi qua AI) cho publish course (tham khảo Udemy)

# OCR
## Log
2026-06-03 09:27:42 [INFO] services.base_service.TextClassifierService: High confidence threats:

 [

    {

        "text": ". 0. Exit PDF and EPUB : Use fitz ( pymupdf ) TextPage. extractBLOCKS ( ), handle both vertical and horizontal text, image is h\u00e9n xui ( quality dependence ), so we ignore it for now. DOCX, T ) sudachipy for tokenizer jamdict for vocab processing ( kanji, kana, translations ) TEXT - TO - SPEECH gITS Lo _",

        "spam_score": 0.9657938480377197,

        "spam_label": "SPAM",

        "toxic_score": 0.7433441877365112,

        "toxic_label": "SAFE"

    }

]

2026-06-03 09:27:42 [INFO] services.base_service.TextClassifierService: Aggregated results:

{"text": ". 0. Exit PDF and EPUB : Use fitz ( pymupdf ) TextPage. extractBLOCKS ( ), handle both vertical and horizontal text, image is h\u00e9n xui ( quality dependence ), so we ignore it for now. DOCX, T ) sudachipy for tokenizer jamdict for vocab processing ( kanji, kana, translations ) TEXT - TO - SPEECH gITS Lo _", "action": "FLAGGED", "score": 0.9657938480377197, "raw_label": "SPAM"}

## Suspect
Desktop/plan.png

## Potential issue
OCR ko quét đc hết hình hoặc do chữ trên hình quá bé
		

# OCR and/or Audio transcript
## Log
2026-06-03 09:28:30 [INFO] services.base_service.TextClassifierService: High confidence threats:
 [

    {

        "text": "nacen :? LITEA LE fothen O & A A @ epppy x m Anan in? Geld @ nacen :? LITEA LE fothen O & A Anan ind Gel\u00ae @ neree? ITS IE fen Oo A Anan Ind CelQ Graves? 1ITEA LE Benen @ A Anan Ind CelQ Graves? 1ITEA LE Benen @ A Anan Ind CelQ Graves? 1ITEA LE Bren @ A @ app. py 1 print < \u00a2 @ vo & & \u20ac ANAND ind ( elf G",

        "spam_score": 0.8746987581253052,

        "spam_label": "SPAM",

        "toxic_score": 0.8684017658233643,

        "toxic_label": "SAFE"

    },

    {

        "text": "##Q Graves? 1ITEA LE Benen @ A Anan Ind CelQ Graves? 1ITEA LE Benen @ A Anan Ind CelQ Graves? 1ITEA LE Bren @ A @ app. py 1 print < \u00a2 @ vo & & \u20ac ANAND ind ( elf Gnares :? lLITE - A LE Bwhon @ & A @ app. py 1 print < \u00a2 @ vo & & \u20ac ANAND ind ( elf Gnares :? lLITE - A LE Bwhon @ & A @ app. py 1 print < \u00a2",

        "spam_score": 0.999777615070343,

        "spam_label": "SPAM",

        "toxic_score": 0.8540061116218567,

        "toxic_label": "SAFE"

    },

    {

        "text": "##nares :? lLITE - A LE Bwhon @ & A @ app. py 1 print < \u00a2 @ vo & & \u20ac ANAND ind ( elf Gnares :? lLITE - A LE Bwhon @ & A @ app. py 1 print < \u00a2 @ vo & & \u20ac ANAND ind ( elf Gnares :? lLITE - A LE Bwhon @ & A @ app. py 1 print < \u00a2 @ vo & & \u20ac ANAND ind ( elf Gnares :? lLITE - A LE Bwhon",

        "spam_score": 0.996327817440033,

        "spam_label": "SPAM",

        "toxic_score": 0.7873209714889526,

        "toxic_label": "SAFE"

    },

    {

        "text": "@ vo & & \u20ac ANAND ind ( elf Gnares :? lLITE - A LE Bwhon @ & A @ app. py 1 print < \u00a2 @ vo & & \u20ac ANAND ind ( elf Gnares :? lLITE - A LE Bwhon @ & A @ app. py 1 print < \u00a2 @ vo & & \u20ac ANAND ind ( elf Gnares :? lLITE - A LE Bwhon @ & A < \u00a2 @ vo & & \u20ac @ app. py 1 print | oS ~ ta EF Summarize",

        "spam_score": 0.9956329464912415,

        "spam_label": "SPAM",

        "toxic_score": 0.8339877128601074,

        "toxic_label": "SAFE"

    },

    {

        "text": "@ & A @ app. py 1 print < \u00a2 @ vo & & \u20ac ANAND ind ( elf Gnares :? lLITE - A LE Bwhon @ & A < \u00a2 @ vo & & \u20ac @ app. py 1 print | oS ~ ta EF Summarize C",

        "spam_score": 0.9997908473014832,

        "spam_label": "SPAM",

        "toxic_score": 0.6927943825721741,

        "toxic_label": "SAFE"

    }

]

2026-06-03 09:28:30 [INFO] services.base_service.TextClassifierService: Aggregated results:

{"text": "@ & A @ app. py 1 print < \u00a2 @ vo & & \u20ac ANAND ind ( elf Gnares :? lLITE - A LE Bwhon @ & A < \u00a2 @ vo & & \u20ac @ app. py 1 print | oS ~ ta EF Summarize C", "action": "FLAGGED", "score": 0.9997908473014832, "raw_label": "SPAM"}			

## Suspect
Desktop/(edit)Recording 2026-05-30 074251.mp4	

## Potential issue
Audio transript seems to not working or Vietnamese is undetectable with the current audio model
	
			
			
		
	

	
