using System;

namespace CourseMarketplaceBE.Domain.Constants
{
    public enum CacheKeys
    {
        UserOnline,
        UserUnread,
        CourseDetail,
        AiModelType,
        MaterialEmbedding,
        MaterialEmbeddingInitialized
    }

    public static class CacheKeysExtensions
    {
        public static string GetKey(this CacheKeys key, object? param1 = null, object? param2 = null)
        {
            return key switch
            {
                CacheKeys.UserOnline => $"user:{param1}:online",
                CacheKeys.UserUnread => $"user:{param1}:unread",
                CacheKeys.CourseDetail => $"course:detail:{param1}",
                CacheKeys.AiModelType => $"ai_models:type:{param1}",
                CacheKeys.MaterialEmbedding => $"material_embedding:{param1}",
                CacheKeys.MaterialEmbeddingInitialized => "material_embedding:initialized",
                _ => throw new ArgumentOutOfRangeException(nameof(key), key, null)
            };
        }
    }

    public enum CacheTtl
    {
        Short,  // 1 hour
        Medium, // 3 hours
    }

    public static class CacheTtlExtensions
    {
        public static TimeSpan GetTtl(this CacheTtl ttl)
        {
            return ttl switch
            {
                CacheTtl.Short => TimeSpan.FromHours(1),
                CacheTtl.Medium => TimeSpan.FromHours(3),
                _ => TimeSpan.FromHours(1)
            };
        }
    }

    public enum CourseStatus
    {
        Draft,
        Pending,
        Published,
        Rejected,
        Flagged,
        Archived
    }

    public static class CourseStatusExtensions
    {
        public static string ToValue(this CourseStatus status)
        {
            return status switch
            {
                CourseStatus.Draft => "draft",
                CourseStatus.Pending => "pending",
                CourseStatus.Published => "published",
                CourseStatus.Rejected => "rejected",
                CourseStatus.Flagged => "flagged",
                CourseStatus.Archived => "archived",
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
        }
    }

    public enum ModerationStatus
    {
        Approved,
        Rejected,
        Flagged,
        ManualAudit,
        Pending
    }

    public static class ModerationStatusExtensions
    {
        public static string ToValue(this ModerationStatus status)
        {
            return status switch
            {
                ModerationStatus.Approved => "APPROVED",
                ModerationStatus.Rejected => "REJECTED",
                ModerationStatus.Flagged => "FLAGGED",
                ModerationStatus.ManualAudit => "MANUAL_AUDIT",
                ModerationStatus.Pending => "PENDING",
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
        }
    }

    public enum StageLogResult
    {
        NoMatch,
        MatchFound,
        Flagged,
        Approved,
        ManualAudit
    }

    public static class StageLogResultExtensions
    {
        public static string ToValue(this StageLogResult result)
        {
            return result switch
            {
                StageLogResult.NoMatch => "NO_MATCH",
                StageLogResult.MatchFound => "MATCH_FOUND",
                StageLogResult.Flagged => "FLAGGED",
                StageLogResult.Approved => "APPROVED",
                StageLogResult.ManualAudit => "MANUAL_AUDIT",
                _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
            };
        }
    }
}
