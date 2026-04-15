using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request); // Sửa kiểu trả về ở đây
    Task<string> RegisterAsync(RegisterRequest request);
}