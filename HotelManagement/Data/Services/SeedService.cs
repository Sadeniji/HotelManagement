using HotelManagement.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Data.Services;

public class SeedService(
    UserManager<ApplicationUser> userManager,
    IUserStore<ApplicationUser> userStore,
    RoleManager<IdentityRole> roleManager,
    IConfiguration configuration,
    IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task SeedDatabaseAsync()
    {
        await using (var context = await contextFactory.CreateDbContextAsync())
        {
            if ((await context.Database.GetPendingMigrationsAsync()).Any())
            {
                await context.Database.MigrateAsync();
            }
        }
        var adminUserEmail = configuration.GetValue<string>("AdminUser:Email");
        configuration.GetValue<string>("AdminUser:Password");

        var adminUser = await userManager.FindByEmailAsync(adminUserEmail!);
        if (adminUser != null)
            return;

        var applicationUser = new ApplicationUser
        {
            FirstName = configuration.GetValue<string>("AdminUser:FirstName")!,
            LastName = configuration.GetValue<string>("AdminUser:LastName"),
            RoleName = RoleType.Admin.ToString(),
            ContactNumber = configuration.GetValue<string>("AdminUser:ContactNumber")!,
            Designation = "Administrator"
        };

        await userStore.SetUserNameAsync(applicationUser, adminUserEmail, default);
        var emailStore = (IUserEmailStore<ApplicationUser>)userStore;
        await emailStore.SetEmailAsync(applicationUser, adminUserEmail, default);

        var result = await userManager.CreateAsync(applicationUser, configuration.GetValue<string>("AdminUser:Password")!);

        if (!result.Succeeded)
        {
            var errors = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description));
            throw new Exception($"Error in creating user. {errors}");
        }

        if (await roleManager.FindByNameAsync(RoleType.Admin.ToString()) == null)
        {
            foreach (var roleName in Enum.GetNames<RoleType>())
            {
                var role = new IdentityRole() { Name = roleName };
                await roleManager.CreateAsync(role);
            }
        }

        var response = await userManager.AddToRoleAsync(applicationUser, RoleType.Admin.ToString());

        if (!response.Succeeded)
        {
            var errors = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description));
            throw new Exception($"Error in adding user to admin roles. {errors}");

        }
    }
}