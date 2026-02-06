using System.Security.Cryptography;
using System.Text;

namespace CommunityCar.Domain.Utilities;

public static class SecurityHelper
{
    public static string HashString(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }

    public static string GenerateRandomToken(int length = 32)
    {
        var buffer = new byte[length];
        RandomNumberGenerator.Fill(buffer);
        return Convert.ToHexString(buffer);
    }

    public static bool VerifyHash(string input, string hash)
    {
        var inputHash = HashString(input);
        return string.Equals(inputHash, hash, StringComparison.OrdinalIgnoreCase);
    }
}
