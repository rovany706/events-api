using System.Security.Claims;
using System.Text;

using EventManager.Application.Abstractions.Security;
using EventManager.Application.Options;
using EventManager.Domain.Entities;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace EventManager.Infrastructure.Security;

/// <summary>
/// Генератор JWT-токенов для пользователей
/// </summary>
public class UserJwtTokenGenerator : IUserJwtTokenGenerator
{
    private readonly UserJwtTokenSettings _tokenSettings;

    public UserJwtTokenGenerator(IOptions<UserJwtTokenSettings> tokenSettings)
    {
        _tokenSettings = tokenSettings.Value;
    }

    /// <inheritdoc />
    public string CreateToken(User user)
    {
        var claims = new Dictionary<string, object> { { JwtRegisteredClaimNames.Sub, user.Id.ToString() } };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var now = DateTime.UtcNow;

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _tokenSettings.Issuer,
            Audience = _tokenSettings.Audience,
            Claims = claims,
            Expires = now.Add(_tokenSettings.Lifetime),
            IssuedAt = now,
            SigningCredentials = creds
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}