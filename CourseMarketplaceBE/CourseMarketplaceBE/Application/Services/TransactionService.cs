using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.IRepositories;

namespace CourseMarketplaceBE.Application.Services;

/// <summary>
/// SOLID — SRP: Chỉ điều phối nghiệp vụ giao dịch.
///   - Không biết EF Core (phụ thuộc ITransactionRepository — DIP).
///   - Không xử lý HTTP (Controller lo — SRP).
///
/// Business Logic:
///   - Validate pagination params (tránh page 0, pageSize quá lớn)
///   - Không transform gì thêm vì Repository đã projection đúng
/// </summary>
public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _repo;
    private const int MaxPageSize = 100;

    public TransactionService(ITransactionRepository repo) => _repo = repo;

    // ═══════════════════════════════════════════════════════════════════════
    // UC-115: DANH SÁCH GIAO DỊCH
    // ═══════════════════════════════════════════════════════════════════════
    public async Task<TransactionPagedResult> GetTransactionsAsync(int page = 1, int pageSize = 20, string? keyword = null, string? sortBy = "date_desc", string? status = null, int? year = null, int? month = null)
    {
        // ★ Guard: tránh invalid pagination
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > MaxPageSize) pageSize = MaxPageSize;

        var (items, totalCount) = await _repo.GetTransactionsAsync(page, pageSize, keyword, sortBy, status, year, month);

        return new TransactionPagedResult
        {
            Items      = items,
            TotalCount = totalCount,
            Page       = page,
            PageSize   = pageSize
        };
    }

    // ═══════════════════════════════════════════════════════════════════════
    // UC-114: CHI TIẾT GIAO DỊCH
    // ═══════════════════════════════════════════════════════════════════════
    public async Task<TransactionDetailDto?> GetTransactionDetailAsync(int transactionId)
    {
        if (transactionId <= 0)
            throw new ArgumentException("Invalid Transaction ID.", nameof(transactionId));

        return await _repo.GetTransactionDetailAsync(transactionId);
    }

    public async Task<TransactionPagedResult> GetInstructorTransactionsAsync(int instructorId, int page = 1, int pageSize = 20, string? keyword = null, string? sortBy = "date_desc", string? status = null, int? year = null, int? month = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > MaxPageSize) pageSize = MaxPageSize;

        var (items, totalCount) = await _repo.GetInstructorTransactionsAsync(instructorId, page, pageSize, keyword, sortBy, status, year, month);

        return new TransactionPagedResult
        {
            Items      = items,
            TotalCount = totalCount,
            Page       = page,
            PageSize   = pageSize
        };
    }

    public async Task<TransactionPagedResult> GetUserTransactionsAsync(int userId, int page = 1, int pageSize = 20, string? keyword = null, string? sortBy = "date_desc", string? status = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > MaxPageSize) pageSize = MaxPageSize;

        // ★ Học viên chỉ xem đơn thành công (không hiện đơn ảo do checkout thất bại)
        var (items, totalCount) = await _repo.GetUserTransactionsAsync(userId, page, pageSize, keyword, sortBy, status);

        return new TransactionPagedResult
        {
            Items      = items,
            TotalCount = totalCount,
            Page       = page,
            PageSize   = pageSize
        };
    }
}
