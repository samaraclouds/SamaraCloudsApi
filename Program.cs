using SamaraCloudsApi.Data;
using SamaraCloudsApi.Helpers;
using SamaraCloudsApi.Middleware;
using SamaraCloudsApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.JsonWebTokens;

var builder = WebApplication.CreateBuilder(args);
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

// [1] Controller
builder.Services.AddControllers();

// [2] Swagger + JWT Auth di Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "SamaraClouds API",
        Version = "v1",
        Description = "API documentation for SamaraClouds system"
    });
    o.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter JWT as: Bearer {token}",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
    });
    o.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// [3] Dependency Injection (tanpa repository, langsung ke service)
builder.Services.AddSingleton<SqlConnectionFactory>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddSingleton<JwtHelper>();
builder.Services.AddScoped<IChartOfAccountService, ChartOfAccountService>();

// [4] JWT Auth Setup with International API Standard Error Handling
var jwtConfig = builder.Configuration.GetSection("Jwt");
var jwtSecret = jwtConfig["Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured");
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production!
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtConfig["Issuer"] ?? "SamaraCloudsApi",
        ValidAudience = jwtConfig["Audience"] ?? "SamaraCloudsApiUsers",
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
    // Professional error handler
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            context.NoResult();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            string errorType, errorMessage;
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                errorType = "token_expired";
                errorMessage = "JWT token has expired.";
            }
            else
            {
                errorType = "invalid_token";
                errorMessage = "JWT token is invalid.";
            }

            var result = JsonSerializer.Serialize(new
            {
                success = false,
                error = errorType,
                message = errorMessage
            });
            return context.Response.WriteAsync(result);
        },
        OnChallenge = context =>
        {
            // Triggered if token missing or Bearer missing in header
            if (!context.Response.HasStarted)
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var result = JsonSerializer.Serialize(new
                {
                    success = false,
                    error = "unauthorized",
                    message = "Authorization header missing or not Bearer token."
                });
                return context.Response.WriteAsync(result);
            }
            return Task.CompletedTask;
        }
    };
});

// [6] CORS jika perlu
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowAll", policy =>
//     {
//         policy.AllowAnyOrigin()
//               .AllowAnyMethod()
//               .AllowAnyHeader();
//     });
// });

var app = builder.Build();

// [7] Pipeline Middleware

// ErrorHandling harus paling atas (supaya catch semua error)
app.UseMiddleware<ErrorHandlingMiddleware>();

// Swagger UI aktif hanya di development (aman untuk production)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(o =>
    {
        o.SwaggerEndpoint("/swagger/v1/swagger.json", "SamaraClouds API v1");
        o.DocumentTitle = "SamaraClouds API Docs";
    });
}

// app.UseCors("AllowAll"); // Uncomment jika aktifkan CORS

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
