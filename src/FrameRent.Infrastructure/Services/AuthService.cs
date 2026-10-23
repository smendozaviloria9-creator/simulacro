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
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            throw new DomainException("Invalid email or password.");

        var isValidPassword = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isValidPassword)
            throw new DomainException("Invalid email or password.");

        var roles = await _userManager.GetRolesAsync(user);
        string primaryRole = roles.FirstOrDefault() ?? "Client";

        string token = await GenerateJwtTokenAsync(user, primaryRole);
        return new AuthResponseDto(token, user.Email!, user.FullName, primaryRole);
    }

    private Task<string> GenerateJwtTokenAsync(ApplicationUser user, string role)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "FrameRentSecretKeyMustBeAtLeast32BytesLong!";
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "FrameRentApi";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "FrameRentUsers";

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
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }
}
