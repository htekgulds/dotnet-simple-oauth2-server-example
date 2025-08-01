using OAuth2.Server.Models;
using System.Text.Json;

namespace OAuth2.Server.Services;

public interface IUserService
{
    Task<User?> ValidateUserAsync(string username, string password);
    Task<User?> GetUserByIdAsync(string userId);
}

public interface ISmsService
{
    Task<bool> SendSmsAsync(string phoneNumber, string message);
}

public interface ITwoFactorService
{
    string GenerateTwoFactorCode();
    Task<bool> SendTwoFactorCodeAsync(string phoneNumber);
    bool ValidateTwoFactorCode(string userId, string code);
    string GenerateTwoFactorToken(string userId, string clientId, string redirectUri, List<string> scopes);
    TwoFactorSession? GetTwoFactorSession(string token);
    void RevokeTwoFactorSession(string token);
}

public class UserService : IUserService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public UserService(HttpClient httpClient, string baseUrl)
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl;
    }

    public async Task<User?> ValidateUserAsync(string username, string password)
    {
        try
        {
            var request = new { Username = username, Password = password };
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/users/validate", request);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<User>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error validating user: {ex.Message}");
        }
        
        return null;
    }

    public async Task<User?> GetUserByIdAsync(string userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/users/{userId}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<User>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error getting user: {ex.Message}");
        }
        
        return null;
    }
}

public class SmsService : ISmsService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public SmsService(HttpClient httpClient, string baseUrl)
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl;
    }

    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            var request = new { PhoneNumber = phoneNumber, Message = message };
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/sms/send", request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error sending SMS: {ex.Message}");
            return false;
        }
    }
}

public class TwoFactorService : ITwoFactorService
{
    private readonly ISmsService _smsService;
    private readonly Dictionary<string, string> _twoFactorCodes = new();
    private readonly Dictionary<string, TwoFactorSession> _twoFactorSessions = new();

    public TwoFactorService(ISmsService smsService)
    {
        _smsService = smsService;
    }

    public string GenerateTwoFactorCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    public async Task<bool> SendTwoFactorCodeAsync(string phoneNumber)
    {
        var code = GenerateTwoFactorCode();
        _twoFactorCodes[phoneNumber] = code;
        
        // Remove code after 5 minutes
        _ = Task.Delay(TimeSpan.FromMinutes(5)).ContinueWith(_ => _twoFactorCodes.Remove(phoneNumber));
        
        var message = $"Your verification code is: {code}";
        return await _smsService.SendSmsAsync(phoneNumber, message);
    }

    public bool ValidateTwoFactorCode(string phoneNumber, string code)
    {
        if (_twoFactorCodes.TryGetValue(phoneNumber, out var storedCode))
        {
            if (storedCode == code)
            {
                _twoFactorCodes.Remove(phoneNumber);
                return true;
            }
        }
        return false;
    }

    public string GenerateTwoFactorToken(string userId, string clientId, string redirectUri, List<string> scopes)
    {
        var token = Guid.NewGuid().ToString();
        var session = new TwoFactorSession
        {
            Token = token,
            UserId = userId,
            ClientId = clientId,
            RedirectUri = redirectUri,
            Scopes = scopes,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };

        _twoFactorSessions[token] = session;
        return token;
    }

    public TwoFactorSession? GetTwoFactorSession(string token)
    {
        if (_twoFactorSessions.TryGetValue(token, out var session))
        {
            if (session.ExpiresAt > DateTime.UtcNow)
            {
                return session;
            }
            _twoFactorSessions.Remove(token);
        }
        return null;
    }

    public void RevokeTwoFactorSession(string token)
    {
        _twoFactorSessions.Remove(token);
    }
}
