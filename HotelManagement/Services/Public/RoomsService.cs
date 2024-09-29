using HotelManagement.Data;
using HotelManagement.Data.Entities;
using HotelManagement.Models.Public;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services.Public;

public interface IRoomsService
{
    Task<RoomTypeModel[]> GetRoomTypesAsync(int count = 0, FilterModel? filter = null);
    Task<LookupModel<Ulid, decimal>[]> GetRoomTypesLookup(FilterModel? filter = null);
}
public class RoomsService(IDbContextFactory<ApplicationDbContext> contextFactory) : IRoomsService
{
    public async Task<RoomTypeModel[]> GetRoomTypesAsync(int count = 0, FilterModel? filter = null)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var query = RoomTypes(filter, context);

        // Get the bookings for these checkin checkout dates
        //check the available room types for the dates

        if (count > 0)
        {
            query = query.Take(count);
        }

        return await query.Select(rt => new RoomTypeModel(
                rt.Id,
                rt.Name,
                rt.Image,
                rt.Description,
                rt.Price,
                rt.Amenities.Select(a => new RoomTypeAmenityModel(a.Amenity.Name, a.Amenity.Icon, a.Unit)).ToArray()))
            .ToArrayAsync();
    }

    public async Task<LookupModel<Ulid, decimal>[]> GetRoomTypesLookup(FilterModel? filter = null)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var query = RoomTypes(filter, context);

        return await query.Select(rt => new LookupModel<Ulid, decimal>(rt.Id, rt.Name, rt.Price)).ToArrayAsync();

    }

    private static IQueryable<RoomType> RoomTypes(FilterModel? filter, ApplicationDbContext context)
    {
        var query = context.RoomTypes.Where(rt => rt.IsActive);

        if (filter?.NumberOfAdults > 0)
        {
            query = query.Where(rt => rt.MaxAdults >= filter.NumberOfAdults);
        }

        if (filter?.NumberOfChildren > 0)
        {
            query = query.Where(rt => rt.MaxChildren >= filter.NumberOfChildren);
        }

        return query;
    }
}