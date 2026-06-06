# Edit confidence_threshold
- Admin change threshold sliders on UI for similarity, spam, toxic, click save
- Save changes to system_config


# Course Creation (might not work as intended with Course Update):

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
		- Create Course object from CourseAIIntegrationCommand
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


- In FastAPI:
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
	- In duplication_service:
		- remove check_exact_duplication
		- move orchestrate_stage1 to handlers layer with some changes
	- In toxicity_service:
		- move orchestrate_stage2 to handlers layer with some changes 
	- In services, add redis_service
	- Add method get_course_details(int course_id) to redis_service:
		- call cache_repository.get_course_data
	- Add method get_existing_embeddings(string key_prefix = 'material_embedding:') to cache_repository:
		- retrieve from redis cache using key_prefix 
	- Add method get_all_existing_embeddings() to redis_service():
		- call cache_repository.get_existing_embeddings()

	- In handlers layer, add files base_handler, stage1_handler, stage2_handler
	- In base_handler, init it similar to base_service with logger and handler_name
	- In stage1_handler: 
		- extends base_handler
		- move orchestrate_stage1 from duplication_service to this
	- In stage2_handler: 
		- extends base_handler
		- move orchestrate_stage2 from toxicity_service to this
	- Modify orchestrate_stage1(int course_id, List[int] material_ids):
		- call embedding_service to generate embeddings
		- call redis_service.get_all_existing_embeddings() to get the existing embeddings (cached from C# BE side)
		- call duplication_service methods to check_semantic_duplication between the newly generated embeddings and the existing embeddings
			- if NOT duplicate -> call embeddings_service to cache the generated embeddings to redis
		- the rest of the method stay the same

	- Modify orchestrate_stage2(int course_id):
		- call redis_service.get_course_details() to get course data from cache (cached from C# BE side)
		- perform toxicity_service.check_text_toxicity on :
			- course.title
			- course.description
			- course.what_you_will_learn
			- course.requirements
			- lesson.title for all lessons in the course
			- material.title, material.description for all materials in every lesson
		- store the current stage_log
		- return immediately if any text_flagged 
		- extract course.thumbnail_url and material.material_url of all learning_materials in the course
		- download the files from cloudinary using the extracted urls
		- perform toxicity_service.check_media_text_toxicity on download material files
		- store the current stage_log
		- return immediately if any media_flagged
		- the rest of the method stay the same
	

	
	
	
# Later
- In BE:
	- add methods AddAsync,Update,Delete,SaveChanges to ISystemConfigRepository and SystemConfigRepository:
	- add methods AddAsync,Update,Delete,SaveChanges, GetAll, GetByName to IAiModelRepository, AiModelRepository:
	- add methods Update,Delete,GetByModelId, GetByCourseId to ICourseAIIntegrationRepository, CourseAIIntegrationRepository
	- add methods Task<CourseAIModerationResult> AiModerationService.CheckSemanticDuplicationAsync(SemanticDuplicationRequest) to IAiModerationService and AiModerationService:
	- add methods Task<CourseAIModerationResult> AiModerationService.CheckHarmfulCourseAsync(CourseHarmfulRequest) to IAiModerationService and AiModerationService:
	- add methods GetAll, GetById to CourseAiUsageLogRepository, ICourseAiUsageLogRepository
	- For course update, need to check for changes by compute hashes for changes (course title, description, what_you_will_learn, reqs, lessons title, description, material byte hash) and validate with existing hashes, if diff -> moderate, else do nothing
	- Need to migrate AiModerationService to Infrastructure since it's an external service (keep the interface in Application layer tho)
	

- In FastAPI:
	- Change models to ONNX format to speed up inference 
	- text_classifier: Use my sliding logic and aggregate_result in text_moderator instead, but apply the parallel and output return
	- cache_repository: Upgrade find_similar_embeddings logic after MVP
	- duplication_service: Use redis cache instead of direct DB query
	- text_extraction_service: 
		- Add extract from csv
		- extract_from_pdf: extract all pdf pages if possible (currently first 5 pages)
		- extract_from_excel: extract all sheets if possible (currently first 5 sheets)
	- (later) Fix CORS block in main.py
	- Modify inference methods to accept hyperparameters as method arguments
	- Need further improvement for manual audit case, currently the code is kinda binary between flagged and approved, manual audit is forgotten and only happens in exception, which is not as intended
	- I gotta figure the differences of REJECTED and FLAGGED for AI moderation
	
Đọc thử bài báo thầy gửi
Clean text, bỏ mạo từ, train lại spam detection model
Train lại 1 em ít params hơn
Xóa phần kiểm tra trùng lặp tiêu đề trong code


# MODFICATION		
need to change the naming convention in BE and FastAPI, use json property thing in BE also 
translate return string to english


get cached ai_models:type:classifer ai_models:type:generator in FastAPI side, this enable the course_ai_usage_log save
fix the return format from FastAPI to match with ModerationService methods (flag, reject, etc.)
there is a huge noticable delay for AI moderation when instructor click submit for review
	after the delay, which most likely when the ai moderation result is returned, the instructor got the pop up notification to wait for 3-5 days 
	this is incorrect, most likely because the entangle between the default manual review and the dogshit AI integration
	need to change the strat, instead of  directly pop up like that, after the hash dedup, we call AI health check 
	if Unhealthy we pop up the "wait for 3-5 days"
	otherwise we pop up "AI moderation in process, you will be notified when it's done", zero waiting
	
enforce unique constraint on every hash cols in course_exts, then we can easily de-dup on course creation/update call, no need to wait until "submit for review"
	ofc, we gonna do this by catching the unique constraint exception
	
improve PrepareMaterialEmbeddingsAsync,SaveMaterialEmbeddingsAsync: 
	instead of load all embeddings when there is
	
Using transactions in db insert, update, delete to ensure data integrity and avoid race condition

request the moderation with cache key instead of just the ids, this way we can centralize the source of possible errors 

Add use case "Setup AI Integration":
	On the UI of AI service management:
		a container contains a form with:
			- list of pre-defined config key on the left and editable config value following dropdown format on the right:
				- click on the config value dropdown would show a dropdown list of available ai_models (already filterd by model_type and process_type match with config key, status = active)
			- a submit button at the bottom-right corner ( label = "Save")
			- on submit -> save the updated config to system_configs following the format:
				- config_key = mapUIStringToDB(Config Key)
				- config_value = model.model_path
		
			
	mapUIStringToDB(UIString) would do something like this:
		Course Harmful Text Classifer -> course_harmful_text_classifier
		Course Text Embedding Generator -> course_text_embedding_generator
		Course Media Embedding Generator -> course_media_embedding_generator
		Review Harmful Text Classifer -> review_harmful_text_classifier
	
on review.comment moderation, first query system_configs table for config_key "review_harmful_text_classifier"

	
# OLD
- In BE, add GenerateManyEmbeddingsRequest to Application\DTOs with the following attributes:
	- int CourseId 
	- List<int> MaterialIds (original list of material_ids belong to the course) 
	
- In BE, add GenerateManyEmbeddingsResponse to Application\DTOs with the following attributes:
	- List<int> MaterialIds (list of material_ids which are associated with successfully generated embeddings)
	
- In FastAPI, add GenerateManyEmbeddingsRequest to core\models with the following attributes:
	- int course_id
	- List[int] material_ids original list of material_ids belong to the course)
	
- In FastAPI, add GenerateManyEmbeddingsResponse to core\models with the following attributes:
	- List[int] material_ids  (list of material_ids which are associated with successfully generated embeddings)
	
- In BE, add methods Task<GenerateManyEmbeddingsResponse> GenerateManyEmbeddingsAsync(GenerateManyEmbeddingsRequest) to IAiModerationService and AiModerationService:
	- This method would call FastAPI side to generate embeddings of learning_materials of course.course_id == CourseId and material_id in MaterialIds:
		- In FastAPI side, use CourseId and MaterialIds to get list of material_url (cloudinary url)
		- Download the files from cloudinary
		- Process them properly based on file_type and generate embeddings
		- Cache the generated embeddings in redis following this structure:
			- key_prefix: "material_embeddings"
			- key_suffix: "{material_id}"
			- "key_prefix:key_suffix" : [embeddings_1, embeddings_2, embeddings_3, etc.]		
			- For now we most likely have 1 material -> 1 embedding, so each material would have a list of only 1 embedding, but this is a generalization for future if we intend to have chunks of embeddings for one material
		- Return the cache key_suffixes to C# BE side