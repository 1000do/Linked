using System;

namespace CourseMarketplaceBE.Domain.Constants
{
    public enum CacheKeys
    {
        UserOnline,
        UserUnread,
        CourseDetail,
        CourseModerationDetail,
        AiModelType,
        MaterialEmbedding,
        MaterialEmbeddingInitialized,
        DuplicateMaterialEmbedding
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
                CacheKeys.CourseModerationDetail => $"course_moderation:detail:{param1}",
                CacheKeys.AiModelType => $"ai_models:type:{param1}",
                CacheKeys.MaterialEmbedding => $"material_embedding:{param1}",
                CacheKeys.MaterialEmbeddingInitialized => "material_embedding:initialized",
                CacheKeys.DuplicateMaterialEmbedding => $"duplicate_embedding:{param1}",
                _ => throw new ArgumentOutOfRangeException(nameof(key), key, "Invalid redis cache key")
            };
        }
    }

    public enum CacheTtl
    {
        Short,  // 1 hour
        Medium, // 3 hours
        Long, // 12 hours
        VeryLong, // 24 hours
    }

    public static class CacheTtlExtensions
    {
        public static TimeSpan GetTtl(this CacheTtl ttl)
        {
            return ttl switch
            {
                CacheTtl.Short => TimeSpan.FromHours(1),
                CacheTtl.Medium => TimeSpan.FromHours(3),
                CacheTtl.Long => TimeSpan.FromHours(12),
                CacheTtl.VeryLong => TimeSpan.FromHours(24),
                _ => TimeSpan.FromHours(1)
            };
        }
    }

    public enum CourseStatus
    {
        Draft,
        Pending,
        Published,
        Removed,
        Rejected,
        Archived,
        PermanentlyLocked
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
                CourseStatus.Removed => "removed",
                CourseStatus.Rejected => "rejected",
                CourseStatus.Archived => "archived",
                CourseStatus.PermanentlyLocked => "permanently_locked",
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Invalid course status")
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
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Invalid moderation status")
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
                _ => throw new ArgumentOutOfRangeException(nameof(result), result, "Invalid AI moderation stage result")
            };
        }
    }

    public enum AIInteractionType{
        Moderation,
        Generation
    }

    public static class AIInteractionTypeExtensions{
        public static string ToValue(this AIInteractionType interactionType){
            return interactionType switch{
                AIInteractionType.Moderation => "moderation",
                AIInteractionType.Generation => "generation",
                _ => throw new ArgumentOutOfRangeException(nameof(interactionType),interactionType, "Invalid AI interaction type")
            };
        }
    }

    public enum InstructorApprovalStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public static class InstructorApprovalStatusExtensions
    {
        public static string ToValue(this InstructorApprovalStatus status)
        {
            return status switch
            {
                InstructorApprovalStatus.Pending => "Pending",
                InstructorApprovalStatus.Approved => "Approved",
                InstructorApprovalStatus.Rejected => "Rejected",
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Invalid instructor approval status")
            };
        }
    }

    public enum StripeOnboardingStatus
    {
        Pending,
        Active
    }

    public static class StripeOnboardingStatusExtensions
    {
        public static string ToValue(this StripeOnboardingStatus status)
        {
            return status switch
            {
                StripeOnboardingStatus.Pending => "Pending",
                StripeOnboardingStatus.Active => "Active",
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Invalid stripe onboarding status")
            };
        }
    }

    public enum LearningStatus
    {
        Active,
        Removed,
        Rejected,
        Flagged
    }

    public static class LearningStatusExtensions
    {
        public static string ToValue(this LearningStatus status)
        {
            return status switch
            {
                LearningStatus.Active => "active",
                LearningStatus.Removed => "removed",
                LearningStatus.Rejected => "rejected",
                LearningStatus.Flagged => "flagged",
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Invalid learning status")
            };
        }
    }

    public enum LessonStatus
    {
        Active,
        Rejected
    }

    public static class LessonStatusExtensions
    {
        public static string ToValue(this LessonStatus status)
        {
            return status switch
            {
                LessonStatus.Active => "active",
                LessonStatus.Rejected => "rejected",
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Invalid lesson status")
            };
        }
    }

    public enum EnrollmentStatus
    {
        Active,
        Revoked
    }

    public static class EnrollmentStatusExtensions
    {
        public static string ToValue(this EnrollmentStatus status)
        {
            return status switch
            {
                EnrollmentStatus.Active => "active",
                EnrollmentStatus.Revoked => "revoked",
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Invalid enrollment status")
            };
        }
    }

    public enum PayoutStatus
    {
        Pending,
        InTransit,
        Paid,
        Failed,
        Transferred,
        Refunded,
        Canceled
    }

    public static class PayoutStatusExtensions
    {
        public static string ToValue(this PayoutStatus status)
        {
            return status switch
            {
                PayoutStatus.Pending => "pending",
                PayoutStatus.InTransit => "in_transit",
                PayoutStatus.Paid => "paid",
                PayoutStatus.Failed => "failed",
                PayoutStatus.Transferred => "transferred",
                PayoutStatus.Refunded => "refunded",
                PayoutStatus.Canceled => "canceled",
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Invalid payout status")
            };
        }
    }

    public enum PlatformWithdrawalStatus
    {
        Pending,
        InTransit,
        Paid,
        Failed,
        Canceled
    }

    public static class PlatformWithdrawalStatusExtensions
    {
        public static string ToValue(this PlatformWithdrawalStatus status)
        {
            return status switch
            {
                PlatformWithdrawalStatus.Pending => "pending",
                PlatformWithdrawalStatus.InTransit => "in_transit",
                PlatformWithdrawalStatus.Paid => "paid",
                PlatformWithdrawalStatus.Failed => "failed",
                PlatformWithdrawalStatus.Canceled => "canceled",
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Invalid platform withdrawal status")
            };
        }
    }

    public enum TransactionStatus
    {
        Pending,
        Succeeded,
        RefundPending,
        Refunded
    }

    public static class TransactionStatusExtensions
    {
        public static string ToValue(this TransactionStatus status)
        {
            return status switch
            {
                TransactionStatus.Pending => "pending",
                TransactionStatus.Succeeded => "succeeded",
                TransactionStatus.RefundPending => "refund_pending",
                TransactionStatus.Refunded => "refunded",
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Invalid transaction status")
            };
        }
    }

    public enum ReportStatus
    {
        Pending,
        Processing,
        Escalated,
        Resolved,
        Rejected,
        UnderReview
    }

    public static class ReportStatusExtensions
    {
        public static string ToValue(this ReportStatus status)
        {
            return status switch
            {
                ReportStatus.Pending => "pending",
                ReportStatus.Processing => "processing",
                ReportStatus.Escalated => "escalated",
                ReportStatus.Resolved => "resolved",
                ReportStatus.Rejected => "rejected",
                ReportStatus.UnderReview => "under_review",
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Invalid report status")
            };
        }
    }

    public enum ReviewStatus
    {
        Ok,
        Removed,
        Violating
    }

    public static class ReviewStatusExtensions
    {
        public static string ToValue(this ReviewStatus status)
        {
            return status switch
            {
                ReviewStatus.Ok => "ok",
                ReviewStatus.Removed => "removed",
                ReviewStatus.Violating => "violating",
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Invalid review status")
            };
        }
    }

    public enum AccountStatus
    {
        Active,
        Banned,
        Flagged1,
        Flagged2
    }

    public static class AccountStatusExtensions
    {
        public static string ToValue(this AccountStatus status)
        {
            return status switch
            {
                AccountStatus.Active => "Active",
                AccountStatus.Banned => "Banned",
                AccountStatus.Flagged1 => "Flagged_1",
                AccountStatus.Flagged2 => "Flagged_2",
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Invalid account status")
            };
        }
    }

    public enum AuthProvider
    {
        Local,
        Google
    }

    public static class AuthProviderExtensions
    {
        public static string ToValue(this AuthProvider provider)
        {
            return provider switch
            {
                AuthProvider.Local => "local",
                AuthProvider.Google => "google",
                _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, "Invalid auth provider")
            };
        }
    }
}
