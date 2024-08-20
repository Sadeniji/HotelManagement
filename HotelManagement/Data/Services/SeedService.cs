using HotelManagement.Constants;
using Microsoft.AspNetCore.Identity;

namespace HotelManagement.Data.Services;

public class SeedService(
    UserManager<ApplicationUser> userManager,
    IUserStore<ApplicationUser> userStore,
    RoleManager<IdentityRole> roleManager,
    IConfiguration configuration)
{
    public async Task SeedDatabaseAsync()
    {
        var adminUserEmail = configuration.GetValue<string>("AdminUser:Email");
        var adminUserPassword = configuration.GetValue<string>("AdminUser:Password");

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

        await userManager.AddToRoleAsync(applicationUser, RoleType.Admin.ToString());

        if (!result.Succeeded)
        {
            var errors = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description));
            throw new Exception($"Error in adding user to admin roles. {errors}");

        }
    }
}