using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FrameRent.Application.DTOs.Auth;
using FrameRent.Application.Interfaces;
using FrameRent.Domain.Exceptions;
using FrameRent.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FrameRent.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            throw new DomainException("Email is already registered.");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            CreatedAtUtc = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            string errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new DomainException($"Registration failed: {errors}");
        }

        // Rule: Anyone registering enters as Client
        await _userManager.AddToRoleAsync(user, "Client");

        string token = await GenerateJwtTokenAsync(user, "Client");
        return new AuthResponseDto(token, user.Email!, user.FullName, "Client");
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        const string invalidCredentials = "Invalid email or password.";

        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new UnauthorizedException(invalidCredentials);

        if (await _userManager.IsLockedOutAsync(user))
            throw new UnauthorizedException("Account temporarily locked due to multiple failed login attempts. Try again later.");

        var isValidPassword = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isValidPassword)
        {
            // Counts the failure; Identity locks the account after MaxFailedAccessAttempts.
            await _userManager.AccessFailedAsync(user);
            throw new UnauthorizedException(invalidCredentials);
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        string primaryRole = roles.FirstOrDefault() ?? "Client";

        string token = await GenerateJwtTokenAsync(user, primaryRole);
        return new AuthResponseDto(token, user.Email!, user.FullName, primaryRole);
    }

    private Task<string> GenerateJwtTokenAsync(ApplicationUser user, string role)
    {
        // No hardcoded fallback: the key must come from user-secrets or environment variables.
        var jwtKey = _configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
            throw new InvalidOperationException("JWT signing key 'Jwt:Key' is missing or shorter than 32 bytes.");

        var jwtIssuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Configuration 'Jwt:Issuer' is missing.");
        var jwtAudience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("Configuration 'Jwt:Audience' is missing.");
        var expirationMinutes = int.TryParse(_configuration["Jwt:ExpirationMinutes"], out var minutes) && minutes > 0
            ? minutes
            : 120;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: creds
        );

        return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }
}
