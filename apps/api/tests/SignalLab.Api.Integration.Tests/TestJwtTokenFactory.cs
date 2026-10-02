using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace SignalLab.Api.Integration.Tests;

internal sealed class TestJwtTokenFactory : IDisposable
{
    private readonly RSA rsa = RSA.Create(2048);
    private readonly RsaSecurityKey signingKey;

    public TestJwtTokenFactory()
    {
        signingKey = new RsaSecurityKey(rsa)
        {
            KeyId = "signallab-integration-tests",
        };
    }

    public SecurityKey ValidationKey => signingKey;

    public string CreateToken(
        string issuer,
        string authorizedParty,
        string subject,
        DateTime? notBefore = null,
        DateTime? expires = null)
    {
        var now = DateTime.UtcNow;

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Subject = new ClaimsIdentity(
            [
                new Claim("sub", subject),
                new Claim("azp", authorizedParty),
            ]),
            IssuedAt = notBefore ?? now,
            NotBefore = notBefore ?? now,
            Expires = expires ?? now.AddMinutes(5),
            SigningCredentials = new SigningCredentials(
                signingKey,
                SecurityAlgorithms.RsaSha256),
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    public void Dispose()
    {
        rsa.Dispose();
    }
}