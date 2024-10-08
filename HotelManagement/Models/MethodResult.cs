﻿namespace HotelManagement.Models;

public readonly record struct MethodResult(bool IsSuccess, string? ErrorMessage)
{
    public static MethodResult Success() => new(true, null);
    public static MethodResult Failure(string errorMessage) => new(false, errorMessage);

    public static implicit operator MethodResult(bool isSuccess) => new (isSuccess, default);
    public static implicit operator MethodResult(string errorMessage) => Failure(errorMessage);
}

public record MethodResult<TData>(bool IsSuccess, string? ErrorMessage, TData Data)
{
    public static MethodResult<TData> Success(TData data) => new(true, null, data);
    public static MethodResult<TData> Failure(string errorMessage) => new(false, errorMessage, default);

    public static implicit operator MethodResult<TData>(TData data) => Success(data);
    //public static implicit operator MethodResult<TData>(string errorMessage) => Failure(errorMessage);
}