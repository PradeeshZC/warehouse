#nullable enable
using Warehouse.Models.DTOs;
using Warehouse.Models.Identity;

namespace Warehouse.Services.Interfaces
{
    public interface IAuthService
    {
        Task<BaseResponse<string>> RegisterAsync(RegisterDto dto);
        Task<BaseResponse<AuthResultDto>> LoginAsync(LoginDto dto);
        Task<BaseResponse<AuthResultDto>> RefreshTokenAsync(string token, string refreshToken);
        Task<BaseResponse<bool>> LogoutAsync(string userId);
    }
}
