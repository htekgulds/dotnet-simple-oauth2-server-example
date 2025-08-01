using OAuth2.Server.Models;
using System.Security.Cryptography;
using System.Text;

namespace OAuth2.Server.Services;

public interface IOAuth2Service
{
    OAuth2Client? GetClient(string clientId);
    bool ValidateClient(string clientId, string clientSecret);
    bool ValidateRedirectUri(string clientId, string redirectUri);
    bool ValidateScope(string clientId, string scope);
    string GenerateAuthorizationCode(string clientId, string userId, string redirectUri, List<string> scopes, string? codeChallenge = null, string? codeChallengeMethod = null);
    AuthorizationCode? GetAuthorizationCode(string code);
    void RevokeAuthorizationCode(string code);
    string GenerateRefreshToken(string clientId, string userId, List<string> scopes);
    RefreshToken? GetRefreshToken(string token);
    void RevokeRefreshToken(string token);
    bool ValidatePKCE(string codeVerifier, string codeChallenge, string codeChallengeMethod);
}

public class OAuth2Service : IOAuth2Service
{
    private readonly List<OAuth2Client> _clients;
    private readonly Dictionary<string, AuthorizationCode> _authorizationCodes = new();
    private readonly Dictionary<string, RefreshToken> _refreshTokens = new();

    public OAuth2Service(List<OAuth2Client> clients)
    {
        _clients = clients;
    }

    public OAuth2Client? GetClient(string clientId)
    {
        return _clients.FirstOrDefault(c => c.ClientId == clientId);
    }

    public bool ValidateClient(string clientId, string clientSecret)
    {
        var client = GetClient(clientId);
        return client != null && client.ClientSecret == clientSecret;
    }

    public bool ValidateRedirectUri(string clientId, string redirectUri)
    {
        var client = GetClient(clientId);
        return client?.RedirectUris.Contains(redirectUri) ?? false;
    }

    public bool ValidateScope(string clientId, string scope)
    {
        var client = GetClient(clientId);
        if (client == null) return false;

        var requestedScopes = scope.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return requestedScopes.All(s => client.AllowedScopes.Contains(s));
    }

    public string GenerateAuthorizationCode(string clientId, string userId, string redirectUri, List<string> scopes, string? codeChallenge = null, string? codeChallengeMethod = null)
    {
        var code = GenerateRandomString(32);
        var authCode = new AuthorizationCode
        {
            Code = code,
            ClientId = clientId,
            UserId = userId,
            RedirectUri = redirectUri,
            Scopes = scopes,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10), // Authorization codes expire in 10 minutes
            CodeChallenge = codeChallenge,
            CodeChallengeMethod = codeChallengeMethod
        };

        _authorizationCodes[code] = authCode;
        return code;
    }

    public AuthorizationCode? GetAuthorizationCode(string code)
    {
        if (_authorizationCodes.TryGetValue(code, out var authCode))
        {
            if (authCode.ExpiresAt > DateTime.UtcNow)
            {
                return authCode;
            }
            // Remove expired code
            _authorizationCodes.Remove(code);
        }
        return null;
    }

    public void RevokeAuthorizationCode(string code)
    {
        _authorizationCodes.Remove(code);
    }

    public string GenerateRefreshToken(string clientId, string userId, List<string> scopes)
    {
        var token = GenerateRandomString(64);
        var refreshToken = new RefreshToken
        {
            Token = token,
            ClientId = clientId,
            UserId = userId,
            Scopes = scopes,
            ExpiresAt = DateTime.UtcNow.AddDays(7) // Refresh tokens expire in 7 days
        };

        _refreshTokens[token] = refreshToken;
        return token;
    }

    public RefreshToken? GetRefreshToken(string token)
    {
        if (_refreshTokens.TryGetValue(token, out var refreshToken))
        {
            if (refreshToken.ExpiresAt > DateTime.UtcNow)
            {
                return refreshToken;
            }
            // Remove expired token
            _refreshTokens.Remove(token);
        }
        return null;
    }

    public void RevokeRefreshToken(string token)
    {
        _refreshTokens.Remove(token);
    }

    public bool ValidatePKCE(string codeVerifier, string codeChallenge, string codeChallengeMethod)
    {
        if (string.IsNullOrEmpty(codeChallenge))
            return true; // PKCE is optional

        if (codeChallengeMethod == "S256")
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
            var computedChallenge = Convert.ToBase64String(hash)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
            return computedChallenge == codeChallenge;
        }
        else if (codeChallengeMethod == "plain")
        {
            return codeVerifier == codeChallenge;
        }

        return false;
    }

    private static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
