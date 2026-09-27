using System.Text;
using KhojMarket.Api.Data;
using KhojMarket.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<ISmsService, DevelopmentSmsService>();
}
else
{
    // Production SMS provider later
    builder.Services.AddScoped<ISmsService, DevelopmentSmsService>();
}

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "KhojMarket API",
            Version = "v1"
        });

    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",

            Type = SecuritySchemeType.Http,

            Scheme = "bearer",

            BearerFormat = "JWT",

            In = ParameterLocation.Header,

            Description =
                "Enter JWT token only."
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type =
                                ReferenceType.SecurityScheme,

                            Id = "Bearer"
                        }
                },
                Array.Empty<string>()
            }
        });
});

builder.Services.AddDbContext<KhojMarketDbContext>(
    options =>
        options.UseSqlServer(
            builder.Configuration
                .GetConnectionString(
                    "DefaultConnection")));

builder.Services.AddScoped<UserService>();

builder.Services.AddScoped<JwtService>();

var jwtSection =
    builder.Configuration.GetSection("Jwt");

var jwtKey =
    jwtSection["Key"]
    ?? throw new InvalidOperationException(
        "JWT Key is missing.");

var jwtIssuer =
    jwtSection["Issuer"]
    ?? throw new InvalidOperationException(
        "JWT Issuer is missing.");

var jwtAudience =
    jwtSection["Audience"]
    ?? throw new InvalidOperationException(
        "JWT Audience is missing.");

builder.Services
    .AddAuthentication(
        options =>
        {
            options.DefaultAuthenticateScheme =
                JwtBearerDefaults
                    .AuthenticationScheme;

            options.DefaultChallengeScheme =
                JwtBearerDefaults
                    .AuthenticationScheme;
        })
    .AddJwtBearer(
        options =>
        {
            options.TokenValidationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuer = true,

                    ValidateAudience = true,

                    ValidateLifetime = true,

                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtIssuer,

                    ValidAudience = jwtAudience,

                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8
                                .GetBytes(jwtKey)),

                    ClockSkew =
                        TimeSpan.Zero
                };
        });

builder.Services.AddAuthorization();
builder.Services.AddScoped<RequirementService>();
builder.Services.AddScoped<OtpService>();
builder.Services.AddScoped<RequirementVerificationService>();
builder.Services.AddScoped<RequirementReadService>();
builder.Services.AddScoped<RequirementLifecycleService>();
builder.Services.AddScoped<RequirementFavoriteService>();
builder.Services.AddScoped<SellerInquiryService>();
builder.Services.AddScoped<SellerCreditService>();
builder.Services.AddScoped<MarketplaceDealService>();
builder.Services.AddScoped<MarketplaceReviewService>();
builder.Services.AddScoped<SellerVerificationService>();
builder.Services.AddScoped<AdminDashboardService>();
var openAiEnabled =
    builder.Configuration.GetValue<bool>(
        "OpenAI:Enabled");

if (openAiEnabled)
{
    builder.Services.AddHttpClient<IAiRequirementService, OpenAiRequirementService>();
}
else
{
    // Safe default: no API key/billing required. Enable real AI explicitly
    // with OpenAI:Enabled=true after production rate limiting is configured.
    builder.Services.AddScoped<IAiRequirementService, DevelopmentAiRequirementService>();
}
builder.Services.AddHealthChecks();
builder.Services.AddScoped<
    IBuyerInquiryNotificationService,
    BuyerInquiryNotificationService>();
var allowedOrigins =
    builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>()
    ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "Frontend",
        policy =>
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}
var logger = app.Services
    .GetRequiredService<ILogger<Program>>();

logger.LogInformation(
    "KhojMarket API starting in {Environment}",
    app.Environment.EnvironmentName);

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseMiddleware<KhojMarket.Api.Middleware.GlobalExceptionMiddleware>();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();