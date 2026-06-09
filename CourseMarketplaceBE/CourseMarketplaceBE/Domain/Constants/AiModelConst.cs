namespace CourseMarketplaceBE.Domain.Constants
{
    public static class AiModelConst
    {
        public const string Moderator = "moderator";
        public const string Generator = "generator";
        public const string EmbeddingGenerator = "embedding_generator";
        public const string Classifier = "classifier";
        public const string Active = "active";
        public const string Inactive = "inactive";
        public const string Similarity = "similarity";
        public const string Spam = "spam";
        public const string Toxic = "toxic";
         // Default fallbacks
        public const float DefaultSimilarityScoreThreshold = 0.8f;
        public const float DefaultSpamScoreThreshold = 0.7f;
        public const float DefaultToxicScoreThreshold = 0.7f;

        
    }
}
