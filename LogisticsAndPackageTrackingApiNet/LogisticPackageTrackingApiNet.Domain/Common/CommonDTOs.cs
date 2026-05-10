using System.ComponentModel.DataAnnotations;

namespace LogisticPackageTrackingApiNet.Domain.Common;

public record ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public List<string>? Errors { get; init; }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Success") 
        => new ApiResponse<T> { Success = true, Data = data, Message = message };

    public static ApiResponse<T> FailureResponse(string message, List<string>? errors = null) 
        => new ApiResponse<T> { Success = false, Message = message, Errors = errors };
}

public record PaginationDTO
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public record FilterDTO
{
    public string? Field { get; init; }
    public string? Operator { get; init; }
    public string? Value { get; init; }
}

public record SearchDTO
{
    public string? Term { get; init; }
    public List<FilterDTO>? Filters { get; init; }
    public PaginationDTO? Pagination { get; init; }
}
