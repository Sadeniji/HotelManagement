using HotelManagement.Constants;
using HotelManagement.Data.Services;

namespace HotelManagement.Endpoints;

public static class Endpoints
{
    public static IEndpointRouteBuilder MapCustomEndpoints(this IEndpointRouteBuilder builder)
    {
        var staffAdminGroup = builder.MapGroup("/staff-admin")
                                    .RequireAuthorization(authPolicyBuilder => 
                                        authPolicyBuilder.RequireRole(RoleType.Admin.ToString(), RoleType.Staff.ToString()));

        staffAdminGroup.MapPost("/manage-amenities/delete/{amenityId}",
            async (string amenityId, IAmenitiesService amenitiesService) =>
            {
                await amenitiesService.DeleteAmenityAsync(Ulid.Parse(amenityId));
                return TypedResults.LocalRedirect("~/staff-admin/manage-amenities");
            });

        return builder;
    }
}