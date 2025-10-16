using DotnetAPI.Data1;
using DotnetAPI.Handler;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "My API", Version = "v1" });

    // ✅ Basic Authentication
    c.AddSecurityDefinition("Basic", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "basic",
        Description = "Basic Authentication using username and password."
    });

    // ✅ API Key Authentication
    c.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "API Key needed to access the endpoints. Use the header 'X-API-KEY: {your_api_key}'",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name = "X-API-KEY",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
    });

    // ✅ Add both schemes to security requirements
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Basic"
                }
            },
            new string[] {}
        },
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            new string[] {}
        }
    });
});


builder.Services.AddCors((options) =>
{
    options.AddPolicy("DevCors", (corsBuilder) =>
        {
            corsBuilder.WithOrigins("http://localhost:4200", "http://localhost:3000", "http://localhost:8000")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    options.AddPolicy("ProdCors", (corsBuilder) =>
        {
            corsBuilder.WithOrigins("https://myProductionSite.com")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
});

builder.Services.AddScoped<IUserRepository,UserRepository>();

string? tokenKeyString = builder.Configuration.GetSection("AppSettings:TokenKey").Value;

SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(tokenKeyString != null ? tokenKeyString : "")
                );

TokenValidationParameters tokenValidationParameters = new TokenValidationParameters()
{
    IssuerSigningKey = tokenKey,
    ValidateIssuer = false,
    ValidateIssuerSigningKey = false,
    ValidateAudience = false
};

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = tokenValidationParameters;
    });

//builder.Services.AddAuthentication("BasicAuthentication")
//    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler2>("BasicAuthentication", null);

//builder.Services.AddAuthentication("ApiKeyAuth")
//    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKeyAuth", null);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "MultiAuth";
    options.DefaultChallengeScheme = "MultiAuth";
})
.AddPolicyScheme("MultiAuth", "Authorize using API Key or Basic", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        string authHeader = context.Request.Headers["Authorization"];

        if (!string.IsNullOrEmpty(authHeader))
        {
            if (authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                return "Basic";

            if (authHeader.StartsWith("ApiKey ", StringComparison.OrdinalIgnoreCase))
                return "ApiKey";
        }

        // If no Authorization header, maybe use X-API-KEY header
        if (context.Request.Headers.ContainsKey("X-API-KEY"))
            return "ApiKey";

        return "Basic"; // default
    };
})
.AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler2>("Basic", null)
.AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", null);
builder.Services.AddAuthorization();


var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevCors");
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHsts();
    app.UseHttpsRedirection();
}
else
{
    app.UseCors("ProdCors");
    app.UseHttpsRedirection();
}





// var app = builder.Build();

// // Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     // app.MapOpenApi();
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }
// else
// {
//    app.UseHttpsRedirection(); 
// }

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();


