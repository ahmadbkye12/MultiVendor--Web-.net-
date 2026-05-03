using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Infrastructure.Identity;
using Infrastructure.Options;
using Infrastructure.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;

public sealed class JwtTokenService(IOptions<JwtSettings> jwtOptions)
{
    private readonly JwtSettings _jwt = jwtOptions.Value;

    public string CreateAccessToken(ApplicationUser user, IEnumerable<string> roles)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var roleList = roles.ToList();
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString())
        };

        claims.AddRange(roleList.Select(r => new Claim(ClaimTypes.Role, r)));

        var expires = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public (string RawRefreshToken, DateTime ExpiresAtUtc) CreateRefreshTokenPayload()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        var raw = Convert.ToBase64String(bytes);
        var expires = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpirationDays);
        return (raw, expires);
    }

    public DateTime GetAccessTokenExpiryUtc() =>
        DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpirationMinutes);

    public string HashRefreshToken(string rawRefreshToken) =>
        TokenHasher.Sha256Hex(rawRefreshToken);
}
