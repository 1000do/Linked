using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices;

/// <summary>
/// SOLID — ISP: Interface nhỏ, chỉ chứa nghiệp vụ liên quan giao dịch.
/// UC-114: Chi tiết giao dịch.
/// UC-115: Danh sách giao dịch có phân trang.
/// </summary>
public interface ITransactionService
{
    /// <summary>
    /// UC-115: Lấy danh sách giao dịch với phân trang.
    /// </summary>
    /// <param name="page">Trang hiện tại (bắt đầu từ 1)</param>
    /// <param name="pageSize">Số item mỗi trang (mặc định 20)</param>
    Task<TransactionPagedResult> GetTransactionsAsync(int page = 1, int pageSize = 20);

    /// <summary>
    /// UC-114: Lấy chi tiết biên lai 1 giao dịch theo ID.
    /// Trả null nếu không tìm thấy.
    /// </summary>
    Task<TransactionDetailDto?> GetTransactionDetailAsync(int transactionId);

    /// <summary>
    /// Lấy danh sách giao dịch của một giảng viên cụ thể.
    /// </summary>
    Task<TransactionPagedResult> GetInstructorTransactionsAsync(int instructorId, int page = 1, int pageSize = 20);
}
