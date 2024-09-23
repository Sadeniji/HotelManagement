using HotelManagement.Data;
using HotelManagement.Data.Entities;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services;

public interface IRoomTypeService
{
    Task<MethodResult<Ulid>> CreateRoomTypeAsync(CreateUpdateRoomType model, string userId);
    Task<GetRoomTypesResponse[]> GetRoomTypesAsync();
    Task<CreateUpdateRoomType?> GetRoomTypeAsync(Ulid roomTypeId);
    Task<Room[]> GetRoomsAsync(Ulid roomTypeId);
    Task<MethodResult<Room>> CreateRoomAsync(Room room);
    Task<MethodResult> DeleteRoomAsync(Ulid roomId);
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

    public async Task<Room[]> GetRoomsAsync(Ulid roomTypeId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        return await context.Rooms
                            .Where(r => r.RoomTypeId == roomTypeId && !r.IsDeleted)
                            .ToArrayAsync();
    }

    public async Task<MethodResult<Room>> CreateRoomAsync(Room room)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync();

            RoomType? roomType;
            if (room.Id == Ulid.Empty)
            {
                if (await context.Rooms.AnyAsync(r => r.RoomNumber == room.RoomNumber && !r.IsDeleted))
                {
                    return MethodResult<Room>.Failure("Room number already exist.");
                }

                if (await context.Rooms.AnyAsync(r => r.RoomNumber == room.RoomNumber))
                {
                    var dbRoom = await context.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == room.RoomNumber);
                    dbRoom.IsAvailable = room.IsAvailable;
                    dbRoom.IsDeleted = true;
                }
                else
                {
                    room.Id = Ulid.NewUlid();
                    await context.Rooms.AddAsync(room);
                }
            }
            else
            {
                var dbRoom = await context.Rooms.FirstOrDefaultAsync(r => r.Id == room.Id && !r.IsDeleted);

                if (dbRoom == null)
                {
                    return MethodResult<Room>.Failure($"Room type with id {room.Id} does not exist.");
                }

                dbRoom.IsAvailable = room.IsAvailable;

            }

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return MethodResult<Room>.Failure(ex.InnerException?.Message ?? ex.Message);
        }
        return MethodResult<Room>.Success(room);
    }

    public async Task<MethodResult> DeleteRoomAsync(Ulid roomId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var dbRoom  = await context.Rooms.FirstOrDefaultAsync(r => r.Id == roomId);

        if (dbRoom is null)
        {
            return $"Room with id {roomId} is not found.";
        }
        dbRoom.IsDeleted = true;
        await context.SaveChangesAsync();

        return true;
    }

}