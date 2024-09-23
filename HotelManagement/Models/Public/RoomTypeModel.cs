namespace HotelManagement.Models.Public;

public readonly record struct RoomTypeAmenityModel(string Name, string? Icon = null, int? Unit = null);

public record RoomTypeModel(Ulid Id, string Name, string Image, string Description, decimal Price, RoomTypeAmenityModel[] Amenities);