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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
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
builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        // This would be tightened up in a real application
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
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
    //if (app.Environment.IsDevelopment())
    //{
    //    await Task.Delay(3000);
    //}
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