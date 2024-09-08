namespace HotelManagement.Models;

public record GetRoomTypesResponse(Ulid Id, string Name, string Image, decimal Price);