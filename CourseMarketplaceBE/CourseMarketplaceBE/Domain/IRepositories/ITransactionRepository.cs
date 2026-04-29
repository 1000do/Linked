using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Domain.IRepositories;

/// <summary>
/// SOLID — ISP: Interface riêng cho transaction queries, không mix vào IAdminFinanceRepository.
/// SOLID — DIP: Service layer chỉ biết interface này, không biết EF Core.
/// </summary>
public interface ITransactionRepository
{
    /// <summary>
    /// UC-115: Lấy danh sách giao dịch có phân trang.
    /// JOIN: transactions → order_items → courses → instructors → users (instructor)
    ///       transactions → account_from → users (buyer)
    ///       transactions → instructor_payouts
    /// </summary>
    Task<(List<TransactionListDto> Items, int TotalCount)> GetTransactionsAsync(int page, int pageSize);

    /// <summary>
    /// UC-114: Lấy chi tiết 1 giao dịch theo ID.
    /// Trả null nếu không tìm thấy.
    /// </summary>
    Task<TransactionDetailDto?> GetTransactionDetailAsync(int transactionId);
}
