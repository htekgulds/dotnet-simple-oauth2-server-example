using Microsoft.AspNetCore.Mvc;
using OAuth2.Server.Models;
using OAuth2.Server.Services;

namespace OAuth2.Server.Controllers;

[ApiController]
[Route("oauth2")]
public class OAuth2Controller : ControllerBase
{
    private readonly IOAuth2Service _oauth2Service;
    private readonly IJwtService _jwtService;
    private readonly IUserService _userService;
    private readonly ITwoFactorService _twoFactorService;

    public OAuth2Controller(
        IOAuth2Service oauth2Service,
        IJwtService jwtService,
        IUserService userService,
        ITwoFactorService twoFactorService)
    {
        _oauth2Service = oauth2Service;
        _jwtService = jwtService;
        _userService = userService;
        _twoFactorService = twoFactorService;
    }

    [HttpGet("authorize")]
    public IActionResult Authorize([FromQuery] AuthorizeRequest request)
    {
        // Validate client
        var client = _oauth2Service.GetClient(request.ClientId);
        if (client == null)
        {
            return BadRequest(new { error = "invalid_client", error_description = "Invalid client_id" });
        }

        // Validate response type
        if (request.ResponseType != "code")
        {
            return BadRequest(new { error = "unsupported_response_type", error_description = "Only 'code' response type is supported" });
        }

        // Validate redirect URI
        if (!_oauth2Service.ValidateRedirectUri(request.ClientId, request.RedirectUri))
        {
            return BadRequest(new { error = "invalid_request", error_description = "Invalid redirect_uri" });
        }

        // Validate scope
        if (!_oauth2Service.ValidateScope(request.ClientId, request.Scope))
        {
            return BadRequest(new { error = "invalid_scope", error_description = "Invalid scope" });
        }

        // Return login page URL with parameters
        var loginUrl = $"/login?client_id={request.ClientId}&redirect_uri={Uri.EscapeDataString(request.RedirectUri)}&scope={Uri.EscapeDataString(request.Scope)}&state={request.State}&code_challenge={request.CodeChallenge}&code_challenge_method={request.CodeChallengeMethod}";
        
        return Ok(new { login_url = loginUrl });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, [FromQuery] string client_id, [FromQuery] string redirect_uri, [FromQuery] string scope, [FromQuery] string? state, [FromQuery] string? code_challenge, [FromQuery] string? code_challenge_method, [FromQuery] string? two_factor_token)
    {
        try
        {
            User? user = null;
            List<string> scopes;

            // Handle 2FA completion
            if (!string.IsNullOrEmpty(two_factor_token) && !string.IsNullOrEmpty(request.TwoFactorCode))
            {
                var twoFactorSession = _twoFactorService.GetTwoFactorSession(two_factor_token);
                if (twoFactorSession == null)
                {
                    return BadRequest(new LoginResponse { Success = false, ErrorMessage = "Invalid or expired 2FA token" });
                }

                user = await _userService.GetUserByIdAsync(twoFactorSession.UserId);
                if (user == null)
                {
                    return BadRequest(new LoginResponse { Success = false, ErrorMessage = "User not found" });
                }

                if (!_twoFactorService.ValidateTwoFactorCode(user.PhoneNumber, request.TwoFactorCode))
                {
                    return BadRequest(new LoginResponse { Success = false, ErrorMessage = "Invalid 2FA code" });
                }

                _twoFactorService.RevokeTwoFactorSession(two_factor_token);
                scopes = twoFactorSession.Scopes;
                client_id = twoFactorSession.ClientId;
                redirect_uri = twoFactorSession.RedirectUri;
            }
            else
            {
                // Regular login
                user = await _userService.ValidateUserAsync(request.Username, request.Password);
                if (user == null)
                {
                    return BadRequest(new LoginResponse { Success = false, ErrorMessage = "Invalid username or password" });
                }

                scopes = scope.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

                // Check if 2FA is required
                if (user.TwoFactorEnabled && string.IsNullOrEmpty(request.TwoFactorCode))
                {
                    await _twoFactorService.SendTwoFactorCodeAsync(user.PhoneNumber);
                    var twoFactorToken = _twoFactorService.GenerateTwoFactorToken(user.Id, client_id, redirect_uri, scopes);
                    
                    return Ok(new LoginResponse 
                    { 
                        Success = false, 
                        RequiresTwoFactor = true, 
                        TwoFactorToken = twoFactorToken 
                    });
                }
            }

            // Generate authorization code
            var authCode = _oauth2Service.GenerateAuthorizationCode(
                client_id, 
                user.Id, 
                redirect_uri, 
                scopes, 
                code_challenge, 
                code_challenge_method);

            return Ok(new LoginResponse 
            { 
                Success = true, 
                AuthorizationCode = authCode 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new LoginResponse { Success = false, ErrorMessage = "Internal server error" });
        }
    }

    [HttpPost("token")]
    public async Task<IActionResult> Token([FromForm] TokenRequest request)
    {
        try
        {
            if (request.GrantType == "authorization_code")
            {
                return await HandleAuthorizationCodeGrant(request);
            }
            else if (request.GrantType == "client_credentials")
            {
                return await HandleClientCredentialsGrant(request);
            }
            else if (request.GrantType == "refresh_token")
            {
                return await HandleRefreshTokenGrant(request);
            }
            else
            {
                return BadRequest(new { error = "unsupported_grant_type", error_description = "Grant type not supported" });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "server_error", error_description = "Internal server error" });
        }
    }

    private async Task<IActionResult> HandleAuthorizationCodeGrant(TokenRequest request)
    {
        // Validate client
        if (!_oauth2Service.ValidateClient(request.ClientId, request.ClientSecret))
        {
            return BadRequest(new { error = "invalid_client", error_description = "Invalid client credentials" });
        }

        // Get and validate authorization code
        var authCode = _oauth2Service.GetAuthorizationCode(request.Code);
        if (authCode == null)
        {
            return BadRequest(new { error = "invalid_grant", error_description = "Invalid or expired authorization code" });
        }

        // Validate redirect URI
        if (authCode.RedirectUri != request.RedirectUri)
        {
            return BadRequest(new { error = "invalid_grant", error_description = "Redirect URI mismatch" });
        }

        // Validate PKCE if present
        if (!string.IsNullOrEmpty(authCode.CodeChallenge))
        {
            if (!_oauth2Service.ValidatePKCE(request.CodeVerifier, authCode.CodeChallenge, authCode.CodeChallengeMethod ?? "plain"))
            {
                return BadRequest(new { error = "invalid_grant", error_description = "PKCE validation failed" });
            }
        }

        // Get user
        var user = await _userService.GetUserByIdAsync(authCode.UserId);
        if (user == null)
        {
            return BadRequest(new { error = "invalid_grant", error_description = "User not found" });
        }

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user, authCode.Scopes);
        var refreshToken = _oauth2Service.GenerateRefreshToken(request.ClientId, user.Id, authCode.Scopes);

        // Revoke authorization code
        _oauth2Service.RevokeAuthorizationCode(request.Code);

        return Ok(new TokenResponse
        {
            AccessToken = accessToken,
            TokenType = "Bearer",
            ExpiresIn = 900, // 15 minutes
            RefreshToken = refreshToken,
            Scope = string.Join(" ", authCode.Scopes)
        });
    }

    private async Task<IActionResult> HandleClientCredentialsGrant(TokenRequest request)
    {
        // Validate client
        if (!_oauth2Service.ValidateClient(request.ClientId, request.ClientSecret))
        {
            return BadRequest(new { error = "invalid_client", error_description = "Invalid client credentials" });
        }

        var client = _oauth2Service.GetClient(request.ClientId);
        if (!client!.AllowedGrantTypes.Contains("client_credentials"))
        {
            return BadRequest(new { error = "unauthorized_client", error_description = "Client not authorized for client_credentials grant" });
        }

        // Validate scope
        var scopes = string.IsNullOrEmpty(request.Scope) ? client.AllowedScopes : request.Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        if (!_oauth2Service.ValidateScope(request.ClientId, string.Join(" ", scopes)))
        {
            return BadRequest(new { error = "invalid_scope", error_description = "Invalid scope" });
        }

        // Create a service user for client credentials
        var serviceUser = new User
        {
            Id = $"service_{request.ClientId}",
            Username = client.ClientName,
            Email = $"{request.ClientId}@service.local",
            FirstName = "Service",
            LastName = "Account"
        };

        // Generate access token (no refresh token for client credentials)
        var accessToken = _jwtService.GenerateAccessToken(serviceUser, scopes);

        return Ok(new TokenResponse
        {
            AccessToken = accessToken,
            TokenType = "Bearer",
            ExpiresIn = 900, // 15 minutes
            Scope = string.Join(" ", scopes)
        });
    }

    private async Task<IActionResult> HandleRefreshTokenGrant(TokenRequest request)
    {
        // Validate client
        if (!_oauth2Service.ValidateClient(request.ClientId, request.ClientSecret))
        {
            return BadRequest(new { error = "invalid_client", error_description = "Invalid client credentials" });
        }

        // Get and validate refresh token
        var refreshTokenData = _oauth2Service.GetRefreshToken(request.RefreshToken);
        if (refreshTokenData == null)
        {
            return BadRequest(new { error = "invalid_grant", error_description = "Invalid or expired refresh token" });
        }

        // Validate client
        if (refreshTokenData.ClientId != request.ClientId)
        {
            return BadRequest(new { error = "invalid_grant", error_description = "Client mismatch" });
        }

        // Get user
        var user = await _userService.GetUserByIdAsync(refreshTokenData.UserId);
        if (user == null)
        {
            return BadRequest(new { error = "invalid_grant", error_description = "User not found" });
        }

        // Generate new tokens
        var accessToken = _jwtService.GenerateAccessToken(user, refreshTokenData.Scopes);
        var newRefreshToken = _oauth2Service.GenerateRefreshToken(request.ClientId, user.Id, refreshTokenData.Scopes);

        // Revoke old refresh token
        _oauth2Service.RevokeRefreshToken(request.RefreshToken);

        return Ok(new TokenResponse
        {
            AccessToken = accessToken,
            TokenType = "Bearer",
            ExpiresIn = 900, // 15 minutes
            RefreshToken = newRefreshToken,
            Scope = string.Join(" ", refreshTokenData.Scopes)
        });
    }
}
