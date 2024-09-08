using HotelManagement.Constants;
using HotelManagement.Data.Entities;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Data.Services;

public interface IRoomTypeService
{
    Task<MethodResult<Ulid>> CreateRoomTypeAsync(CreateUpdateRoomType model, string userId);
    Task<GetRoomTypesResponse[]> GetRoomTypesAsync();
    Task<CreateUpdateRoomType?> GetRoomTypeAsync(Ulid roomTypeId);
}

public class RoomTypeService(IDbContextFactory<ApplicationDbContext> contextFactory) : IRoomTypeService
{
    public async Task<MethodResult<Ulid>> CreateRoomTypeAsync(CreateUpdateRoomType model, string userId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        RoomType? roomType;
        if (model.Id == Ulid.Empty)
        {
            if (await context.RoomTypes.AnyAsync(rt => rt.Name.ToLower() == model.Name.ToLower()))
            {
                return MethodResult<Ulid>.Failure($"Room type with the same name {model.Name} already exists.");
            }

            roomType = new RoomType
            {
                Id = Ulid.NewUlid(),
                Name = model.Name,
                AddedBy = userId,
                AddedOn = DateTime.UtcNow,
                Description = model.Description,
                Image = model.Image,
                IsActive = model.IsActive,
                MaxAdults = model.MaxAdults,
                MaxChildren = model.MaxChildren,
                Price = model.Price,
                LastUpdatedBy = userId,
                LastUpdatedOn = DateTime.UtcNow,
            };

            await context.RoomTypes.AddAsync(roomType);
        }
        else
        {
            if (await context.RoomTypes.AnyAsync(rt => rt.Name.ToLower() == model.Name.ToLower() && rt.Id != model.Id))
            {
                return MethodResult<Ulid>.Failure($"Room type with the same name {model.Name} already exists.");
            }

            roomType = await context.RoomTypes.FirstOrDefaultAsync(rt => rt.Id == model.Id);
            if (roomType == null)
            {
                return MethodResult<Ulid>.Failure($"Invalid request.");
            }

            roomType.Name = model.Name;
            roomType.Description = model.Description;
            roomType.Image = model.Image;
            roomType.IsActive = model.IsActive;
            roomType.MaxAdults = model.MaxAdults;
            roomType.MaxChildren = model.MaxChildren;
            roomType.Price = model.Price;
            roomType.LastUpdatedBy = userId;
            roomType.LastUpdatedOn = DateTime.UtcNow;

            var existingRoomTypeAmenities = await context.RoomTypeAmenities.Where(rt => rt.RoomTypeId == model.Id).ToListAsync();
            context.RoomTypeAmenities.RemoveRange(existingRoomTypeAmenities);
        }

        await context.SaveChangesAsync();

        if (model.Amenities.Length > 0)
        {
            var roomTypeAmenities = model.Amenities.Select(a => new RoomTypeAmenity
            {
                AmenityId = a.Id,
                RoomTypeId = roomType.Id,
                Unit = a.Unit
            });
            await context.RoomTypeAmenities.AddRangeAsync(roomTypeAmenities);
            await context.SaveChangesAsync();
        }
        return roomType.Id;
    }

    public async Task<GetRoomTypesResponse[]> GetRoomTypesAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.RoomTypes
            .Select(rt => new GetRoomTypesResponse(rt.Id, rt.Name, rt.Image, rt.Price))
            .ToArrayAsync();
    }
    
    public async Task<CreateUpdateRoomType?> GetRoomTypeAsync(Ulid roomTypeId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var roomType = await context.RoomTypes
                                    .Include(rt => rt.Amenities)
                                    .Where(rt => rt.Id == roomTypeId)
                                    .Select(rt => new CreateUpdateRoomType
                                    {
                                        Id = rt.Id,
                                        Description = rt.Description,
                                        Name = rt.Name,
                                        Price = rt.Price,
                                        MaxAdults = rt.MaxAdults,
                                        MaxChildren = rt.MaxChildren,
                                        Image = rt.Image,
                                        IsActive = rt.IsActive,
                                        Amenities = rt.Amenities.Select(a => new CreateUpdateRoomTypeAmenity(a.AmenityId, a.Unit)).ToArray()
                                    }).FirstOrDefaultAsync();

        return roomType;
    }
}