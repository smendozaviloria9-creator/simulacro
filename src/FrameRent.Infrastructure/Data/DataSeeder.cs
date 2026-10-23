using FrameRent.Domain.Entities;
using FrameRent.Infrastructure.Identity;
using FrameRent.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FrameRent.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await context.Database.MigrateAsync();

        // 1. Roles
        string[] roles = ["Staff", "Client"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Staff user required on first run
        string staffEmail = "staff@framerent.com";
        var staffUser = await userManager.FindByEmailAsync(staffEmail);
        if (staffUser == null)
        {
            staffUser = new ApplicationUser
            {
                UserName = staffEmail,
                Email = staffEmail,
                FullName = "FrameRent Staff Admin",
                EmailConfirmed = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(staffUser, "Staff@123456");
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(staffUser, "Staff");
            }
        }

        // 3. Demo Client user
        string clientEmail = "client@framerent.com";
        var clientUser = await userManager.FindByEmailAsync(clientEmail);
        if (clientUser == null)
        {
            clientUser = new ApplicationUser
            {
                UserName = clientEmail,
                Email = clientEmail,
                FullName = "Demo Content Creator",
                EmailConfirmed = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(clientUser, "Client@123456");
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(clientUser, "Client");
            }
        }

        // 4. Sample Equipment
        if (!await context.Equipments.AnyAsync())
        {
            var initialEquipment = new List<Equipment>
            {
                new()
                {
                    Name = "Sony FX3 Cinema Camera",
                    Category = "Cameras",
                    Description = "Full-frame cinema line camera with 4K 120p recording, high dynamic range.",
                    DailyRate = 85.00m,
                    TotalUnits = 4,
                    IsActive = true
                },
                new()
                {
                    Name = "Canon EOS R5 C",
                    Category = "Cameras",
                    Description = "8K mirrorless cinema camera with non-stop video recording.",
                    DailyRate = 95.00m,
                    TotalUnits = 3,
                    IsActive = true
                },
                new()
                {
                    Name = "Aputure 300d II LED Light",
                    Category = "Lighting",
                    Description = "Daylight balanced 5500K point-source LED fixture with Bowens mount.",
                    DailyRate = 35.00m,
                    TotalUnits = 6,
                    IsActive = true
                },
                new()
                {
                    Name = "Rode Wireless PRO Microphone System",
                    Category = "Audio",
                    Description = "Dual-channel wireless microphone system with 32-bit float on-board recording.",
                    DailyRate = 25.00m,
                    TotalUnits = 8,
                    IsActive = true
                },
                new()
                {
                    Name = "Manfrotto 504X Video Tripod Kit",
                    Category = "Tripods & Grip",
                    Description = "Fluid video head with carbon fiber legs for heavy video setups.",
                    DailyRate = 20.00m,
                    TotalUnits = 5,
                    IsActive = true
                }
            };

            await context.Equipments.AddRangeAsync(initialEquipment);
            await context.SaveChangesAsync();
        }
    }
}
