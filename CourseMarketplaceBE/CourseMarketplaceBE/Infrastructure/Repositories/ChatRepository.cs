using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Infrastructure.Repositories;

public class ChatRepository : IChatRepository
{
    private readonly AppDbContext _context;

    public ChatRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ChatParticipant>> GetParticipantsByAccountIdAsync(int accountId)
    {
        return await _context.ChatParticipants
            .Where(p => p.AccountId == accountId)
            .Include(p => p.Chat)
                .ThenInclude(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
            .Include(p => p.Chat)
                .ThenInclude(c => c.ChatParticipants)
                    .ThenInclude(cp => cp.Account)
                        .ThenInclude(a => a.User)
            .Include(p => p.Chat)
                .ThenInclude(c => c.ChatParticipants)
                    .ThenInclude(cp => cp.Account)
                        .ThenInclude(a => a.Manager)
            .OrderByDescending(p => p.Chat.LastMessageAt)
            .ToListAsync();
    }

    public async Task<List<Message>> GetMessagesByChatIdAsync(int chatId)
    {
        return await _context.Messages
            .Where(m => m.ChatId == chatId)
            .Include(m => m.Sender)
                .ThenInclude(a => a.User)
            .Include(m => m.Sender)
                .ThenInclude(a => a.Manager)
            .OrderBy(m => m.SentAt)
            .ToListAsync();
    }

    public async Task AddMessageAsync(Message message)
    {
        _context.Messages.Add(message);
        
        // Tăng unread count cho những người khác trong phòng
        var otherParticipants = await _context.ChatParticipants
            .Where(p => p.ChatId == message.ChatId && p.AccountId != message.SenderId)
            .ToListAsync();

        foreach (var p in otherParticipants)
        {
            p.UnreadCount++;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<Chat?> GetChatByIdAsync(int chatId)
    {
        return await _context.Chats.FindAsync(chatId);
    }

    public async Task UpdateChatAsync(Chat chat)
    {
        _context.Entry(chat).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task<Chat> CreateChatAsync(Chat chat)
    {
        _context.Chats.Add(chat);
        await _context.SaveChangesAsync();
        return chat;
    }

    public async Task AddChatParticipantsAsync(IEnumerable<ChatParticipant> participants)
    {
        _context.ChatParticipants.AddRange(participants);
        await _context.SaveChangesAsync();
    }

    public async Task<ChatParticipant?> FindPrivateChatAsync(int accountId1, int accountId2, string? contextType, int? contextId)
    {
        return await _context.ChatParticipants
            .Where(p => p.AccountId == accountId1)
            .Where(p => p.Chat.ChatType == "private")
            .Where(p => p.Chat.ContextId == contextId && p.Chat.ContextType == contextType)
            .FirstOrDefaultAsync(p => _context.ChatParticipants
                .Any(p2 => p2.ChatId == p.ChatId && p2.AccountId == accountId2));
    }

    public async Task<bool> IsParticipantAsync(int chatId, int accountId)
    {
        return await _context.ChatParticipants
            .AnyAsync(p => p.ChatId == chatId && p.AccountId == accountId);
    }

    public async Task<bool> HasActiveReportForChatAsync(int chatId)
    {
        return await _context.UserReports
            .AnyAsync(r => r.ChatId == chatId && 
                          (r.UserReportsStatus == "pending" || r.UserReportsStatus == "processing"));
    }

    public async Task UpdateAdminAccessAsync(int chatId, DateTime expiry)
    {
        var report = await _context.UserReports
            .Where(r => r.ChatId == chatId && (r.UserReportsStatus == "pending" || r.UserReportsStatus == "processing"))
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();

        if (report != null)
        {
            report.AccessGrantedUntil = expiry;
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddReportAsync(UserReport report)
    {
        _context.UserReports.Add(report);
        await _context.SaveChangesAsync();
    }

    public async Task AddAuditLogAsync(AuditLog log)
    {
        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task MarkAsReadAsync(int chatId, int accountId)
    {
        var participant = await _context.ChatParticipants
            .FirstOrDefaultAsync(p => p.ChatId == chatId && p.AccountId == accountId);

        if (participant != null)
        {
            participant.UnreadCount = 0;
            participant.LastReadAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetTotalUnreadCountAsync(int accountId)
    {
        return await _context.ChatParticipants
            .Where(p => p.AccountId == accountId)
            .SumAsync(p => p.UnreadCount);
    }
}
