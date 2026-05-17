namespace CourseMarketplaceBE.Domain.Constants
{
    public static class SystemConfigKeys
    {
        public const string SimilarityScoreThreshold = "similarity_score_threshold";
        public const string SpamScoreThreshold = "spam_score_threshold";
        public const string ToxicScoreThreshold = "toxic_score_threshold";
        public const string ModerationThreshold = "moderation_threshold";

        // Default fallbacks
        public const float DefaultSimilarityScoreThreshold = 0.8f;
        public const float DefaultSpamScoreThreshold = 0.7f;
        public const float DefaultToxicScoreThreshold = 0.7f;
    }
}
