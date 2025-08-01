using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OAuth2.Server.Models;
using OAuth2.Server.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
var oauth2Clients = builder.Configuration.GetSection("OAuth2Clients").Get<List<OAuth2Client>>()!;
var externalServices = builder.Configuration.GetSection("ExternalServices").Get<ExternalServices>()!;

builder.Services.AddSingleton(jwtSettings);
builder.Services.AddSingleton(oauth2Clients);
builder.Services.AddSingleton(externalServices);

// Register services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IOAuth2Service, OAuth2Service>();
builder.Services.AddScoped<ITwoFactorService, TwoFactorService>();

// Register HTTP clients for external services
builder.Services.AddHttpClient<IUserService, UserService>((serviceProvider, client) =>
{
    var services = serviceProvider.GetRequiredService<ExternalServices>();
    client.BaseAddress = new Uri(services.UserServiceUrl);
});

builder.Services.AddHttpClient<ISmsService, SmsService>((serviceProvider, client) =>
{
    var services = serviceProvider.GetRequiredService<ExternalServices>();
    client.BaseAddress = new Uri(services.SmsServiceUrl);
});

// Configure JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
