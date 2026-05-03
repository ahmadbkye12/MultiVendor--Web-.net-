using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Security;

internal static class TokenHasher
{
    public static string Sha256Hex(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
