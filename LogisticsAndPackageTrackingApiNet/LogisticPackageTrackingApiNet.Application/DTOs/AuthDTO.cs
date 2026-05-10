namespace LogisticPackageTrackingApiNet.Application.DTOs;

public record AuthDTO
{
    public string Mail { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public record RegisterDTO
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Mail { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
}

public record AuthResponseDTO
{
    public string Token { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Mail { get; init; } = string.Empty;
}

public record UserResponseDTO
{
    public string Token { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Mail { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
}
