namespace HotelManagement.Data.Entities;

public class RoomTypeAmenity
{
    public Ulid RoomTypeId { get; set; }
    public Ulid AmenityId { get; set; }
    public int? Unit { get; set; }

    public virtual RoomType RoomType { get; set; }
    public virtual Amenity Amenity { get; set; }
}