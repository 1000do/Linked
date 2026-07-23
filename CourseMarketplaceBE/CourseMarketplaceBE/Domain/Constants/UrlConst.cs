namespace CourseMarketplaceBE.Domain.Constants
{
public static class UrlConst{
    public const string AdminCourseModerationURL = "/AdminModeration/Courses";
    
    public const string AIModerationBaseURL = "http://ai-moderation:8000";
    public const string HealthCheckURL = "health";
    public const string FullPipelineURL = "moderation/full-pipeline";
    public const string ReviewModerationURL = "moderation/review-comment";
    // public const string SemanticDeDuplicationURL = "";
    // public const string DeToxificationURL = "";
    // public const string EmbeddingGenerationURL = "";
}
}
