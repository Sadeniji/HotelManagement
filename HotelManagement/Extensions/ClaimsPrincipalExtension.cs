using System.Security.Claims;

namespace HotelManagement.Extensions;

public static class ClaimsPrincipalExtension
{
    public static string? GetUserId(this ClaimsPrincipal principal) => principal.FindFirstValue(ClaimTypes.NameIdentifier);
}