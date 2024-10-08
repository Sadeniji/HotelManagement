﻿namespace HotelManagement.Models;

public record UserDisplayModel(
    string Id, 
    string Name, 
    string Email, 
    string RoleName,
    string ContactNumber,
    string? Designation);