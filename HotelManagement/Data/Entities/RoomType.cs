﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Data.Entities;

public class RoomType
{
    public Ulid Id { get; set; }

    [Required, MaxLength(100), Unicode(false)]
    public string Name { get; set; }

    [Required, MaxLength(100), Unicode(false)]
    public string Image { get; set; }

    [Required, Range(1, double.MaxValue)]
    public decimal Price { get; set; }

    [Required, MaxLength(200), Unicode(false)]
    public string Description { get; set; }

    public int MaxAdults { get; set; }
    public int MaxChildren { get; set; }

    public bool IsActive { get; set; }

    public DateTime AddedOn { get; set; }

    [Required]
    public string AddedBy { get; set; }

    public DateTime? LastUpdatedOn { get; set; }

    [Unicode(false)]
    public string LastUpdatedBy { get; set; }

    [ForeignKey(nameof(AddedBy))]
    public virtual ApplicationUser AddedByUser { get; set; }

    public virtual ICollection<RoomTypeAmenity> Amenities { get; set; }
    public virtual ICollection<Room> Rooms { get; set; }
}