using HotelManagement.Data.Entities;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace HotelManagement.Data.Services;

public interface IAmenitiesService
{
    Task<Amenity[]> GetAmenitiesAsync();
    Task<MethodResult<Amenity>> SaveAmenityAsync(Amenity amenity);
    Task<MethodResult<bool>> DeleteAmenityAsync(Ulid id);
}

public class AmenitiesService(IDbContextFactory<ApplicationDbContext> contextFactory) : IAmenitiesService
{
    public async Task<Amenity[]> GetAmenitiesAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Amenities.Where(a => !a.IsDeleted).AsNoTracking().ToArrayAsync();
    }

    public async Task<MethodResult<Amenity>> SaveAmenityAsync(Amenity amenity)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        if (amenity.Id == Ulid.Empty)
        {
            if (await context.Amenities.AnyAsync(a => a.Name.ToLower().Trim() == amenity.Name.ToLower().Trim() &&
                                                      !a.IsDeleted))
            {
                return MethodResult<Amenity>.Failure("Amenity with the same name already exists.");
            }
            amenity.Id = Ulid.NewUlid();
            await context.Amenities.AddAsync(amenity);
        }
        else
        {
            if (await context.Amenities.AnyAsync(a => a.Name.ToLower().Trim() == amenity.Name.ToLower().Trim() &&
                                                      a.Id != amenity.Id &&
                                                      !a.IsDeleted))
            {
                return MethodResult<Amenity>.Failure("Amenity with the same name already exists.");
            }
            var dbAmenity = await context.Amenities
                                        .FirstOrDefaultAsync(a => a.Id == amenity.Id)
                            ?? throw new InvalidOperationException("Amenity does not exist");
            
            dbAmenity.Name = amenity.Name;
            dbAmenity.Icon = amenity.Icon;
        }
        await context.SaveChangesAsync();
        return amenity;
        //return MethodResult<Amenity>.Success(amenity);
    }

    public async Task<MethodResult<bool>> DeleteAmenityAsync(Ulid id)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var amenity = await context.Amenities.FirstOrDefaultAsync(a => a.Id == id);

        if (amenity == null) return false;

        amenity.IsDeleted = true;
        await context.SaveChangesAsync();
        return true;
    }
}