using SamaraCloudsApi.Data;
using SamaraCloudsApi.Helpers;
using SamaraCloudsApi.Middleware;
using SamaraCloudsApi.Models;
using SamaraCloudsApi.Services;
using SamaraCloudsApi.Extensions; // <= TAMBAHKAN INI
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

// [1] Controller + Filter global + Validasi model standart
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiResponseWrapperFilter>();
})
.ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        var errorResponse = new ApiErrorResponse
        {
            Status = StatusCodes.Status400BadRequest,
            Code = "VALIDATION_ERROR",
            Message = "One or more validation errors occurred.",
            Errors = errors
        };

        return new BadRequestObjectResult(errorResponse);
    };
});

// [2] Swagger + JWT di Swagger
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

// [3] Dependency Injection
builder.Services.AddSingleton<SqlConnectionFactory>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddSingleton<JwtHelper>();
builder.Services.AddScoped<IChartOfAccountService, ChartOfAccountService>();

// [4] JWT Config
var jwtConfig = builder.Configuration.GetSection("Jwt");

string? jwtSecretRaw = jwtConfig["Secret"];
if (string.IsNullOrWhiteSpace(jwtSecretRaw))
    throw new InvalidOperationException("JWT Secret is not configured");

string jwtSecret = jwtSecretRaw;
var key = Encoding.ASCII.GetBytes(jwtSecret);

string? issuerRaw = jwtConfig["Issuer"];
string issuer = string.IsNullOrWhiteSpace(issuerRaw) ? "SamaraCloudsApi" : issuerRaw;

string? audienceRaw = jwtConfig["Audience"];
string audience = string.IsNullOrWhiteSpace(audienceRaw) ? "SamaraCloudsApiUsers" : audienceRaw;

// [5] Authentication with JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            context.NoResult();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            string code = "INVALID_TOKEN";
            string message = "JWT token is invalid.";

            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                code = "TOKEN_EXPIRED";
                message = "JWT token has expired.";
            }

            var errorResponse = new ApiErrorResponse
            {
                Status = context.Response.StatusCode,
                Code = code,
                Message = message,
                Errors = context.Exception.Message
            };

            var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            return context.Response.WriteAsync(json);
        },
        OnChallenge = context =>
        {
            if (!context.Response.HasStarted)
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var errorResponse = new ApiErrorResponse
                {
                    Status = context.Response.StatusCode,
                    Code = "UNAUTHORIZED",
                    Message = "Authorization header missing or not Bearer token.",
                    Errors = null
                };

                var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });

                return context.Response.WriteAsync(json);
            }
            return Task.CompletedTask;
        }
    };
});

// [6] CORS jika diperlukan
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

// [7] Middleware global (wajib di atas routing & auth)
app.UseMiddleware<ErrorHandlingMiddleware>();

// [8] Routing Khusus (Custom redirect endpoint friendly)
app.MapDocRedirects();

app.MapGet("/", context =>
{
    context.Response.Redirect("/docs");
    return Task.CompletedTask;
});

// [9] Static Files & Swagger
app.UseDefaultFiles(); // untuk index.html
app.UseStaticFiles();

// --- SWAGGER UI DILINDUNGI ADMIN ---
app.UseWhen(ctx => ctx.Request.Path.StartsWithSegments("/swagger") 
    && !ctx.Request.Path.Value!.EndsWith("swagger.json"), swaggerApp =>
{
    swaggerApp.Use(async (context, next) =>
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }

        // Hanya allow admin (ubah "Admin" ke role lain jika perlu)
        if (!context.User.IsInRole("Admin"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Forbidden");
            return;
        }

        await next();
    });
});


app.UseSwagger();
app.UseSwaggerUI(o =>
{
    o.SwaggerEndpoint("/swagger/v1/swagger.json", "SamaraClouds API v1");
    o.DocumentTitle = "SamaraClouds API Docs";
});

// [10] Auth
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
