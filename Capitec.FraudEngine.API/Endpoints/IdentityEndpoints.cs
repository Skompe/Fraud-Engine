using Capitec.FraudEngine.API.Extensions;
using Capitec.FraudEngine.Application.Features.Identity.Login;
using Capitec.FraudEngine.Application.Features.Identity.RegisterUser;
using Capitec.FraudEngine.Application.Features.Identity.RefreshToken;
using MediatR;

namespace Capitec.FraudEngine.API.Endpoints
{
    public record LoginRequest(string Username, string Password);
    public record LoginResponse(string AccessToken, string TokenType, int ExpiresIn);
    public record RefreshTokenRequest(string RefreshToken);
    public record RegisterUserRequest(string Username, string Password, string Role);
    public static class IdentityEndpoints
    {
        public static void MapIdentityEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/identity").WithTags("Identity");

            group.MapPost("/login", async (LoginRequest request, ISender mediator, CancellationToken ct) =>
            {
                var query = new LoginQuery(request.Username, request.Password);

                var result = await mediator.Send(query, ct);

                return result.IsError ? result.ToProblemDetails() : Results.Ok(result.Value);
            });

            group.MapPost("/refresh", async (RefreshTokenRequest request, ISender mediator, CancellationToken ct) =>
            {
                var command = new RefreshTokenCommand(request.RefreshToken);

                var result = await mediator.Send(command, ct);

                return result.IsError ? result.ToProblemDetails() : Results.Ok(result.Value);
            })
            .WithName("RefreshTokenHandler")
            .WithOpenApi()
            .WithDescription("Refresh access token using a valid refresh token");
            

            group.MapPost("/register", async (RegisterUserRequest request, ISender sender, CancellationToken ct) =>
            {
                var command = new RegisterUserCommand(request.Username, request.Password, request.Role);

                var result = await sender.Send(command, ct);

                if (result.IsError)
                {
                    return result.ToProblemDetails();
                }

                var safeResponse = new
                {
                    Id = result.Value.Id,
                    Username = result.Value.Username,
                    Role = result.Value.Role,
                    IsActive = result.Value.IsActive,
                    CreatedAt = result.Value.CreatedAt
                };

                return Results.Ok(safeResponse);
            });
        }
    }
}
