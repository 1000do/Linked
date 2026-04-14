using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices;

public interface IUserService
{
    Task<string> RegisterAsync(RegisterRequest request);
    Task<string?> LoginAsync(LoginRequest request); // Thêm Login
    Task<string> GetFullNameByEmailAsync(string email); // Thêm dòng này
}