using System.Security.Claims;
using ContosoExpense.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ContosoExpense.Services;

/// <summary>
/// Interface for mock authentication service.
/// </summary>
public interface IAuthService
{
    Task<User?> GetCurrentUserAsync();
    Task<bool> IsManagerAsync();
    Task SwitchUserAsync(HttpContext context, string userId);
    string? GetCurrentUserId();
}

/// <summary>
/// Mock authentication service for demo purposes.
/// </summary>
public class MockAuthService : IAuthService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserRepository _userRepository;

    public MockAuthService(IHttpContextAccessor httpContextAccessor, IUserRepository userRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _userRepository = userRepository;
    }

    public string? GetCurrentUserId()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return null;

        return await _userRepository.GetByIdAsync(userId);
    }

    public async Task<bool> IsManagerAsync()
    {
        var user = await GetCurrentUserAsync();
        return user?.Role == UserRole.Manager;
    }

    public async Task SwitchUserAsync(HttpContext context, string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return;

        // Sign out current user
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // Create claims for new user
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.GivenName, user.DisplayName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
        };

        await context.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }
}
