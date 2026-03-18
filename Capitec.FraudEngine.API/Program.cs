using Capitec.FraudEngine.API.Endpoints;
using Capitec.FraudEngine.Application;
using Capitec.FraudEngine.Infrastructure;
using Capitec.FraudEngine.Infrastructure.Persistence;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.RateLimiting;
using Capitec.FraudEngine.Domain.Abstractions.Rules;
using Capitec.FraudEngine.API.Infrastructure;
using Capitec.FraudEngine.API.Constants;
using Capitec.FraudEngine.API.Authorization;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new Microsoft.OpenApi.Models.OpenApiComponents();

        
        var scheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = SecurityConstants.Swagger.Scheme,
            BearerFormat = SecurityConstants.Swagger.BearerFormat,
            Description = SecurityConstants.Swagger.TokenDescription
        };

        document.Components.SecuritySchemes.Add(SecurityConstants.Swagger.SchemeName, scheme);

        
        document.SecurityRequirements.Add(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            [new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = SecurityConstants.Swagger.SchemeName
                }
            }] = Array.Empty<string>()
        });

        return Task.CompletedTask;
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
            };
        });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(SecurityConstants.Policies.FraudRead, policy =>
        policy.Requirements.Add(new ScopeRequirement(SecurityConstants.Policies.FraudRead)));
    
    options.AddPolicy(SecurityConstants.Policies.FraudWrite, policy =>
        policy.Requirements.Add(new ScopeRequirement(SecurityConstants.Policies.FraudWrite)));
});

builder.Services.AddSingleton<IAuthorizationHandler, ScopeAuthorizationHandler>();

var configuredCorsOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?.Where(origin => !string.IsNullOrWhiteSpace(origin))
    .Select(origin => origin.Trim())
    .ToArray() ?? [];

var defaultDevelopmentOrigins = new[]
{
    "http://localhost:3000",
    "http://localhost:5173",
    "http://localhost:8080"
};

var allowedCorsOrigins = configuredCorsOrigins.Length > 0
    ? configuredCorsOrigins
    : builder.Environment.IsDevelopment()
        ? defaultDevelopmentOrigins
        : [];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (allowedCorsOrigins.Length == 0)
        {
            return;
        }

        policy.WithOrigins(allowedCorsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(SecurityConstants.Policies.StrictRateLimit, opt =>
    {
        opt.Window = TimeSpan.FromSeconds(10);
        opt.PermitLimit = 1000;
    });
});

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<FraudDbSeeder>();
    await seeder.SeedAsync(app.Environment.IsProduction());

    var ruleManager = scope.ServiceProvider.GetRequiredService<IFraudRuleManager>();
    await ruleManager.InitializeRulesAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Capitec Fraud Engine API")
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);

    });
}
app.UseExceptionHandler();
app.UseRateLimiter();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

//Public
app.MapIdentityEndpoints();

//Protected
app.MapTransactionEndpoints();
app.MapInvestigationEndpoints();
app.MapRuleEndpoints();
app.MapCustomerEndpoints();

app.Run();

public partial class Program { }