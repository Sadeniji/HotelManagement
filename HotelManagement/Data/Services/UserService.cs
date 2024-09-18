using HotelManagement.Constants;
using HotelManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Data.Services;

public interface IUserService
{
    Task<MethodResult<ApplicationUser>> CreateUserAsync(ApplicationUser user, string email, string password);
    Task<MethodResult> UpdateUserAsync(EditStaffModel model, CancellationToken cancellationToken);
    Task<PagedResult<UserDisplayModel>> GetUsersAsync(int startIndex, int pageSize, RoleType? roleType = null);
    Task<EditStaffModel?> GetStaffMemberAsync(string staffId, CancellationToken cancellationToken);
}

public class UserService(
    IDbContextFactory<ApplicationDbContext> contextFactory,
    UserManager<ApplicationUser> userManager,
    IUserStore<ApplicationUser> userStore) : IUserService
{

    public async Task<PagedResult<UserDisplayModel>> GetUsersAsync(int startIndex, int pageSize, RoleType? roleType = null)
    {
        var users = userManager.Users;

        if (roleType != null)
        {
            users = users.Where(u => u.RoleName == roleType.ToString());
        }

        var total = await users.CountAsync();

        users = users.OrderByDescending(u => u.FirstName)
                     .Skip(startIndex)
                     .Take(pageSize);

        var userRecords =  await users.Select(u => new UserDisplayModel(
                                                                                u.Id, 
                                                                                u.FullName, 
                                                                                u.Email, 
                                                                                u.RoleName,
                                                                                u.ContactNumber,
                                                                                u.Designation)).ToArrayAsync();

        return new PagedResult<UserDisplayModel>(total, userRecords);
    }

    public async Task<MethodResult> UpdateUserAsync(EditStaffModel model, CancellationToken cancellationToken)
    {
        var existingUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == model.Id, cancellationToken);

        if (existingUser == null)
        {
            return MethodResult.Failure("Staff is not found");
        }

        existingUser.FirstName =  model.FirstName;
        existingUser.LastName =  model.LastName;
        existingUser.Designation =  model.Designation;
        existingUser.ContactNumber =  model.ContactNumber;

        var result = await userManager.UpdateAsync(existingUser);

        if (!result.Succeeded)
        {
            var errorMessage = $"Error: {string.Join(",", result.Errors.Select(error => error.Description))}";
            return MethodResult.Failure(errorMessage);
        }

        if (existingUser.Email != null && !existingUser.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase))
        {
            var changeEmailToken = await userManager.GenerateChangeEmailTokenAsync(existingUser, model.Email);

            result = await userManager.ChangeEmailAsync(existingUser, model.Email, changeEmailToken);

            if (!result.Succeeded)
            {
                var errorMessage = $"Error: {string.Join(",", result.Errors.Select(error => error.Description))}";
                return MethodResult.Failure(errorMessage);
            }

            return MethodResult.Success();
        }

        return MethodResult.Success();
    }

    public async Task<EditStaffModel?> GetStaffMemberAsync(string staffId, CancellationToken cancellationToken)
    {
        var staff = await userManager.Users
                                                   .FirstOrDefaultAsync(u => u.Id == staffId && u.RoleName == RoleType.Staff.ToString(), cancellationToken);
        return staff == null ? null : new EditStaffModel
                                    {
                                        Id = staffId,
                                        FirstName = staff.FirstName,
                                        LastName = staff.LastName,
                                        Email = staff.Email ?? string.Empty,
                                        Designation = staff.Designation,
                                        ContactNumber = staff.ContactNumber
                                    };
    }

    public async Task<MethodResult<ApplicationUser>> CreateUserAsync(ApplicationUser user, string email, string password)
    {
        await userStore.SetUserNameAsync(user, email, CancellationToken.None);
        var emailStore = GetEmailStore();
        await emailStore.SetEmailAsync(user, email, CancellationToken.None);

        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var errorMessage = $"Error: {string.Join(",", result.Errors.Select(error => error.Description))}";
            return MethodResult<ApplicationUser>.Failure(errorMessage);
        }

        result = await userManager.AddToRoleAsync(user, user.RoleName ?? RoleType.Guest.ToString());

        if (!result.Succeeded)
        {
            var errorMessage = $"Error: {string.Join(",", result.Errors.Select(error => error.Description))}";
            return MethodResult<ApplicationUser>.Failure(errorMessage);
        }

        return user;
    }

    private IUserEmailStore<ApplicationUser> GetEmailStore()
    {
        if (!userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }

        return (IUserEmailStore<ApplicationUser>)userStore;
    }
}