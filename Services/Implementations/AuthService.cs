#nullable enable
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Warehouse.Models.DTOs;
using Warehouse.Models.Identity;
using Warehouse.Models.Entities;
using Warehouse.Services.Interfaces;
using Warehouse.Repositories.Interfaces;

namespace Warehouse.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtService _jwtService;
        private readonly IUnitOfWorkAsync _unitOfWork;
        private readonly IRepository<RefreshToken> _refreshTokenRepository;

        public AuthService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IJwtService jwtService, IUnitOfWorkAsync unitOfWork, IRepository<RefreshToken> refreshTokenRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtService = jwtService;
            _unitOfWork = unitOfWork;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<BaseResponse<string>> RegisterAsync(RegisterDto dto)
        {
            // Normalize/trim inputs to avoid false duplicates due to whitespace/casing
            dto.Email = dto.Email?.Trim() ?? string.Empty;
            dto.UserName = dto.UserName?.Trim() ?? string.Empty;

            // Check email uniqueness first
            var existingByEmail = await _userManager.FindByEmailAsync(dto.Email);
            if (existingByEmail != null)
                return Result.Fail<string>("Email already in use");

            // Check username uniqueness explicitly to provide clear error message
            var existingByName = await _userManager.FindByNameAsync(dto.UserName);
            if (existingByName != null)
                return Result.Fail<string>("Username already in use");

            var user = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                FullName = dto.FullName
            };

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                return Result.Fail<string>(errors);
            }

            // Ensure default role exists and assign it
            if (!await _roleManager.RoleExistsAsync("Staff"))
            {
                var roleCreate = await _roleManager.CreateAsync(new IdentityRole("Staff"));
                if (!roleCreate.Succeeded)
                {
                    var errors = string.Join("; ", roleCreate.Errors.Select(e => e.Description));
                    return Result.Fail<string>($"Failed to create default role: {errors}");
                }
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, "Staff");
            if (!addRoleResult.Succeeded)
            {
                var errors = string.Join("; ", addRoleResult.Errors.Select(e => e.Description));
                return Result.Fail<string>($"Failed to assign role: {errors}");
            }

            return Result.Ok(user.Id);
        }

        public async Task<BaseResponse<AuthResultDto>> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Result.Fail<AuthResultDto>("Invalid credentials");

            var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordValid)
                return Result.Fail<AuthResultDto>("Invalid credentials");

            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<System.Security.Claims.Claim>(userClaims);
            claims.AddRange(roles.Select(r => new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, r)));
            claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id));

            var accessToken = _jwtService.GenerateAccessToken(user, claims);
            var refreshToken = _jwtService.GenerateRefreshToken();

            var refreshEntity = new RefreshToken
            {
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                UserId = user.Id
            };

            await _refreshTokenRepository.AddAsync(refreshEntity);
            await _unitOfWork.CompleteAsync();

            return Result.Ok(new AuthResultDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60)
            });
        }

        public async Task<BaseResponse<AuthResultDto>> RefreshTokenAsync(string token, string refreshToken)
        {
            var principal = _jwtService.GetPrincipalFromExpiredToken(token);
            if (principal == null)
                return Result.Fail<AuthResultDto>("Invalid token");

            var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Result.Fail<AuthResultDto>("Invalid token");

            // Validate refresh token
            var stored = (await _refreshTokenRepository.FindAsync(rt => rt.UserId == userId && rt.Token == refreshToken)).FirstOrDefault();
            if (stored == null || stored.ExpiresAt < DateTime.UtcNow || stored.RevokedAt != null)
                return Result.Fail<AuthResultDto>("Invalid refresh token");

            // revoke old
            stored.RevokedAt = DateTime.UtcNow;
            await _refreshTokenRepository.UpdateAsync(stored);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result.Fail<AuthResultDto>("User not found");

            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<System.Security.Claims.Claim>(userClaims);
            claims.AddRange(roles.Select(r => new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, r)));
            claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id));

            var newAccess = _jwtService.GenerateAccessToken(user, claims);
            var newRefresh = _jwtService.GenerateRefreshToken();

            var newRefreshEntity = new RefreshToken
            {
                Token = newRefresh,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                UserId = user.Id
            };

            await _refreshTokenRepository.AddAsync(newRefreshEntity);
            await _unitOfWork.CompleteAsync();

            return Result.Ok(new AuthResultDto
            {
                AccessToken = newAccess,
                RefreshToken = newRefresh,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60)
            });
        }

        public async Task<BaseResponse<bool>> LogoutAsync(string userId)
        {
            // Revoke all refresh tokens for the user
            var tokens = await _refreshTokenRepository.FindAsync(rt => rt.UserId == userId && rt.RevokedAt == null);
            foreach (var t in tokens)
            {
                t.RevokedAt = DateTime.UtcNow;
                await _refreshTokenRepository.UpdateAsync(t);
            }

            await _unitOfWork.CompleteAsync();
            return Result.Ok(true);
        }
    }
}
