using System.Security.Cryptography;
using System.Text;

using EventManager.Application.Abstractions.Security;

namespace EventManager.Application.Security;

/// <summary>
/// Хеширование пароля с помощью SHA-256
/// </summary>
public class SHA256PasswordHasher : IPasswordHasher
{
    /// <inheritdoc />
    public string Hash(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));

        return Convert.ToHexString(bytes);
    }

    /// <inheritdoc />
    public bool Verify(string password, string expectedHash)
    {
        var passwordHash = Hash(password);

        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(passwordHash),
            Encoding.UTF8.GetBytes(expectedHash));
    }
}