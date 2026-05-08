using System;
using System.Collections.Generic;

namespace CourseMarketplaceBE.Application.DTOs;

public class ChatListDto
{
    public int ChatId { get; set; }
    public string? ChatName { get; set; }
    public string ChatType { get; set; } = null!;
    public string? LastMessage { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public string? PartnerName { get; set; }
    public string? PartnerAvatar { get; set; }
    public string? ContextType { get; set; }
    public int? ContextId { get; set; }
    public int UnreadCount { get; set; }
}

public class MessageDto
{
    public int MessageId { get; set; }
    public int ChatId { get; set; }
    public int? SenderId { get; set; }
    public string SenderName { get; set; } = null!;
    public string? SenderAvatar { get; set; }
    public string Content { get; set; } = null!;
    public bool IsSeen { get; set; }
    public string MessageStatus { get; set; } = null!;
    public DateTime? SentAt { get; set; }
    public List<AttachmentDto> Attachments { get; set; } = new();
}

public class AttachmentDto
{
    public int AttachmentId { get; set; }
    public string FileUrl { get; set; } = null!;
    public string? FileName { get; set; }
    public string? FileType { get; set; }
    public long? FileSize { get; set; }
}

public class SendMessageDto
{
    public int ChatId { get; set; }
    public string Content { get; set; } = null!;
    public List<AttachmentInputDto>? Attachments { get; set; }
}

public class AttachmentInputDto
{
    public string FileUrl { get; set; } = null!;
    public string? FileName { get; set; }
    public string? FileType { get; set; }
    public long? FileSize { get; set; }
}

public class CreateChatDto
{
    public int TargetAccountId { get; set; }
    public string? ContextType { get; set; }
    public int? ContextId { get; set; }
}

public class SubmitReportDto
{
    public int ChatId { get; set; }
    public string Reason { get; set; } = null!;
    public string Description { get; set; } = null!;
}
