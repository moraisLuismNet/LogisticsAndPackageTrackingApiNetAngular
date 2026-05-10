using LogisticPackageTrackingApiNet.Domain.Entities;

namespace LogisticPackageTrackingApiNet.Api.Helpers;

public static class AuthHelpers
{
    public static string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password));
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public static bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordHash))
            return false;
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
