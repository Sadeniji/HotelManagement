using HotelManagement.Data;
using HotelManagement.Models.Public;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services.Public;

public interface IRoomsService
{
    Task<RoomTypeModel[]> GetRoomTypesAsync(int count = 0);
}
public class RoomsService(IDbContextFactory<ApplicationDbContext> contextFactory) : IRoomsService
{
    public async Task<RoomTypeModel[]> GetRoomTypesAsync(int count = 0)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var query = context.RoomTypes
                                                      .Where(rt => rt.IsActive)
                                                      .Select(rt => 
                                                          new RoomTypeModel(
                                                              rt.Id, 
                                                              rt.Name,
                                                              rt.Image,
                                                              rt.Description, 
                                                              rt.Price,
                                                              rt.Amenities.Select(a => new RoomTypeAmenityModel(a.Amenity.Name, a.Amenity.Icon, a.Unit)).ToArray()));

        if (count > 0)
        {
            query = query.Take(count);
        }
        return await query.ToArrayAsync();
    }
}