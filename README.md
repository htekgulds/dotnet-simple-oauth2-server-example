# Simple OAuth2 Server with .NET 8

A comprehensive OAuth2 authorization server implementation with JWT tokens, 2FA support, and a React demo application.

## Architecture

This solution consists of 4 projects:

1. **OAuth2.Server** (Port 7000) - Main OAuth2 authorization server
2. **UserService.Mock** (Port 7002) - Mock user authentication service
3. **SmsService.Mock** (Port 7003) - Mock SMS service for 2FA
4. **DemoApp** (Port 7001) - React + .NET demo application

## Features

- ✅ OAuth2 Authorization Code Flow with PKCE
- ✅ OAuth2 Client Credentials Flow  
- ✅ JWT Access and Refresh Tokens
- ✅ Two-Factor Authentication (2FA) with SMS
- ✅ Protected API Endpoints
- ✅ React OAuth2 Client Integration
- ✅ Multiple OAuth2 Clients Configuration
- ✅ Modern React UI with Bootstrap

## Quick Start

### Prerequisites

- .NET 8 SDK
- Node.js (for React app)

### Running the Services

1. **Start all services in separate terminals:**

```bash
# Terminal 1 - OAuth2 Server
cd OAuth2.Server
dotnet run

# Terminal 2 - User Service
cd UserService.Mock
dotnet run

# Terminal 3 - SMS Service
cd SmsService.Mock
dotnet run

# Terminal 4 - Demo App
cd DemoApp
dotnet run
```

2. **Access the applications:**
   - OAuth2 Server: https://localhost:7000
   - Demo App: https://localhost:7001
   - User Service: https://localhost:7002
   - SMS Service: https://localhost:7003

## Test Accounts

| Username   | Password    | 2FA Enabled | Description |
|------------|-------------|-------------|-------------|
| john.doe   | password123 | ✅ Yes      | User with 2FA |
| jane.smith | password456 | ❌ No       | Regular user |
| admin      | admin123    | ✅ Yes      | Admin user |

## OAuth2 Flows

### Authorization Code Flow (with PKCE)

1. Navigate to Demo App: https://localhost:7001
2. Click "Login with OAuth2"
3. You'll be redirected to the OAuth2 server login page
4. Enter credentials from test accounts above
5. If 2FA is enabled, enter the 6-digit code (check console logs of SMS service)
6. You'll be redirected back to the demo app with an access token

### Client Credentials Flow

Use the following client for API-to-API authentication:

```
Client ID: api-client
Client Secret: api-client-secret
Scopes: api
```

Example request:
```bash
curl -X POST https://localhost:7000/oauth2/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials&client_id=api-client&client_secret=api-client-secret&scope=api"
```

## API Endpoints

### OAuth2 Server (Port 7000)

- `GET /oauth2/authorize` - Start authorization flow
- `POST /oauth2/login` - User login with 2FA support
- `POST /oauth2/token` - Token exchange endpoint

### Demo App (Port 7001)

- `GET /api/public` - Public endpoint (no auth required)
- `GET /api/protected` - Protected endpoint (requires valid JWT)
- `GET /api/admin` - Admin endpoint (requires admin scope)

### User Service (Port 7002)

- `POST /api/users/validate` - Validate user credentials
- `GET /api/users/{id}` - Get user by ID
- `GET /api/users` - Get all users

### SMS Service (Port 7003)

- `POST /api/sms/send` - Send SMS (mock)
- `GET /api/sms/logs` - View SMS logs
- `DELETE /api/sms/logs` - Clear SMS logs

## Configuration

### OAuth2 Clients

Configured in `OAuth2.Server/appsettings.json`:

```json
{
  "OAuth2Clients": [
    {
      "ClientId": "demo-web-app",
      "ClientSecret": "demo-web-app-secret",
      "ClientName": "Demo Web Application",
      "AllowedGrantTypes": ["authorization_code", "refresh_token"],
      "RedirectUris": ["https://localhost:7001/callback"],
      "AllowedScopes": ["openid", "profile", "email", "api"]
    },
    {
      "ClientId": "api-client",
      "ClientSecret": "api-client-secret",
      "ClientName": "API Client",
      "AllowedGrantTypes": ["client_credentials"],
      "AllowedScopes": ["api"]
    }
  ]
}
```

### JWT Settings

```json
{
  "JwtSettings": {
    "SecretKey": "MyVerySecretKeyForJWTTokenGeneration123456789",
    "Issuer": "SimpleOAuth2Server",
    "Audience": "SimpleOAuth2Client",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

## Development

### Project Structure

```
├── OAuth2.Server/           # OAuth2 authorization server
│   ├── Controllers/         # OAuth2 and login controllers
│   ├── Models/             # OAuth2 models and DTOs
│   ├── Services/           # JWT, OAuth2, and external services
│   └── Views/              # Login page view
├── UserService.Mock/        # Mock user service
│   ├── Controllers/        # User API controller
│   └── Models/             # User models
├── SmsService.Mock/         # Mock SMS service
│   ├── Controllers/        # SMS API controller
│   └── Models/             # SMS models
└── DemoApp/                # React + .NET demo app
    ├── Controllers/        # Demo API controller
    └── ClientApp/          # React application
        ├── src/
        │   ├── components/ # React components
        │   └── services/   # OAuth service
        └── public/
```

### Adding New OAuth2 Clients

1. Add client configuration to `appsettings.json`
2. Update redirect URIs as needed
3. Configure allowed scopes and grant types

### Customizing 2FA

The SMS service is mocked and logs codes to the console. To integrate with a real SMS provider:

1. Update `SmsService.Mock` to call actual SMS API
2. Implement proper code storage and validation
3. Add rate limiting and security measures

## Security Considerations

⚠️ **This is a demo implementation. For production use:**

- Use proper password hashing (bcrypt, Argon2)
- Implement rate limiting
- Use secure random number generation
- Store secrets in secure configuration
- Implement proper logging and monitoring
- Add input validation and sanitization
- Use HTTPS in production
- Implement proper CORS policies

## Troubleshooting

### Common Issues

1. **CORS Errors**: Ensure all services are running on correct ports
2. **JWT Validation Errors**: Check that JWT settings match between services
3. **2FA Code Issues**: Check SMS service console logs for generated codes
4. **React Build Issues**: Run `npm install` in `DemoApp/ClientApp` directory

### Logs

Check console output of each service for detailed logs and 2FA codes.

## License

This project is for educational purposes. Use at your own risk in production environments.
