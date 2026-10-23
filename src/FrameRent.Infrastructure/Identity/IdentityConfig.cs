using Microsoft.AspNetCore.Identity;

namespace FrameRent.Infrastructure.Identity;

/// <summary>
/// Shared Identity security settings used by both the API and the Web app,
/// so password and lockout policies cannot drift apart.
/// </summary>
public static class IdentityConfig
{
    public static void ConfigureOptions(IdentityOptions options)
    {
        // Password policy
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;

        // Brute-force protection
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

        options.User.RequireUniqueEmail = true;
    }
}
