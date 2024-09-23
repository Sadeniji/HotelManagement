using HotelManagement.Constants;
using HotelManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Data.Services;

public interface IUserService
{
    Task<MethodResult<ApplicationUser>> CreateUserAsync(ApplicationUser user, string email, string password);
    Task<MethodResult> UpdateUserProfileAsync(ProfileModel model, RoleType? roleType, CancellationToken cancellationToken);
    Task<PagedResult<UserDisplayModel>> GetUsersAsync(int startIndex, int pageSize, RoleType? roleType = null);
    Task<ProfileModel?> GetUserProfileDetailsAsync(string userId, CancellationToken cancellationToken);
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

    public async Task<MethodResult> UpdateUserProfileAsync(ProfileModel model, RoleType? roleType, CancellationToken cancellationToken)
    {
        var existingUser = await GetUser(model.Id, roleType).FirstOrDefaultAsync(cancellationToken);

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

    public async Task<ProfileModel?> GetUserProfileDetailsAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await userManager.Users
                                    .FirstOrDefaultAsync(u => u.Id == userId && (u.RoleName == RoleType.Staff.ToString() || 
                                                         u.RoleName == RoleType.Admin.ToString()), cancellationToken);
        
        return user == null ? null : new ProfileModel
        {
            Id = userId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            Designation = user.Designation,
            ContactNumber = user.ContactNumber
        };
        
    }

    private IQueryable<ApplicationUser> GetUser(string userId, RoleType? roleType = null)
    {
        var query = userManager.Users.Where(u => u.Id == userId);

        return roleType != null ? query.Where(u => u.RoleName == roleType.ToString()) :  query;
        
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