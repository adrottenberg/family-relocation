using System.Security.Cryptography;
using System.Text;

namespace FamilyRelocation.Infrastructure.AWS.Helpers;

internal static class HmacHelper
{
    public static string ComputeSecretHash(string username, string clientId, string clientSecret)
    {
        var message = username + clientId;
        var keyBytes = Encoding.UTF8.GetBytes(clientSecret);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(messageBytes);
        return Convert.ToBase64String(hashBytes);
    }
}
