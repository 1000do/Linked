using System;
using System.Collections.Generic;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class Account
{
    public int AccountId { get; set; }

    public string Email { get; set; } = null!;

    public string? PasswordHash { get; set; }

    public string? PhoneNumber { get; set; }

    public string? AccountStatus { get; set; }

    public int? AccountFlagCount { get; set; }

    public string? AuthProvider { get; set; }

    public string? AvatarUrl { get; set; }

    public DateTime? AccountCreatedAt { get; set; }

    public DateTime? AccountUpdatedAt { get; set; }

    public DateTime? AccountLastLoginAt { get; set; }

    public virtual Manager? Manager { get; set; }

    public virtual ICollection<Message> MessageReceivers { get; set; } = new List<Message>();

    public virtual ICollection<Message> MessageSenders { get; set; } = new List<Message>();

    public virtual ICollection<Notification> NotificationReceivers { get; set; } = new List<Notification>();

    public virtual ICollection<Notification> NotificationSenders { get; set; } = new List<Notification>();

    public virtual User? User { get; set; }

    public virtual ICollection<UserReport> UserReportReporters { get; set; } = new List<UserReport>();

    public virtual ICollection<UserReport> UserReportResolvers { get; set; } = new List<UserReport>();

    public virtual ICollection<UserReport> UserReportTargets { get; set; } = new List<UserReport>();
}
