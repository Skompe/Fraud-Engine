using Capitec.FraudEngine.Application.Features.Identity.Login;
using Capitec.FraudEngine.Application.Features.Identity.RegisterUser;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Capitec.FraudEngine.API.Endpoints
{
    public record LoginRequest(string Username, string Password);
    public record LoginResponse(string AccessToken, string TokenType, int ExpiresIn);
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

                return result.Match(
                    response => Results.Ok(response),
                    errors => Results.Unauthorized()
                );
            });
            

            group.MapPost("/register", async (RegisterUserRequest request, ISender sender, CancellationToken ct) =>
            {
                
                var command = new RegisterUserCommand(request.Username, request.Password, request.Role);

                var result = await sender.Send(command, ct);

              
                if (result.IsError)
                {
                    
                    if (result.FirstError.Type == ErrorOr.ErrorType.Conflict)
                    {
                        return Results.Conflict(new { Message = result.FirstError.Description });
                    }

                    
                    var validationErrors = result.Errors
                        .GroupBy(e => e.Code)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());

                    return Results.ValidationProblem(validationErrors);
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
