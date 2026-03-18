using Capitec.FraudEngine.API.Extensions;
using Capitec.FraudEngine.Application.Features.RulesManagement.CreateRule;
using Capitec.FraudEngine.Application.Features.RulesManagement.GetActiveRules;
using Capitec.FraudEngine.Application.Features.RulesManagement.GetRuleByName;
using Capitec.FraudEngine.Application.Features.RulesManagement.ToggleRuleStatus;
using Capitec.FraudEngine.Application.Features.RulesManagement.UpdateRule;
using Capitec.FraudEngine.Application.Features.RulesManagement.UpdateRuleExpression;
using Capitec.FraudEngine.Application.Features.RulesManagement.UpdateRuleThreshold;
using Capitec.FraudEngine.Domain.Abstractions.Rules;
using Capitec.FraudEngine.API.Constants;
using Capitec.FraudEngine.Infrastructure.Managers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Capitec.FraudEngine.API.Endpoints
{
    public record ToggleRuleRequest(bool IsActive);
    public record UpdateExpressionRequest(string NewExpression);
    public record CreateRuleRequest(string RuleName, string Description, string Expression);
    public record UpdateThresholdRequest(string Parameters);
    public record UpdateRuleRequest(string Description, string? Expression, string? Parameters, bool IsActive);
    public static class RuleEndpoints
    {
        public static void MapRuleEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/rules").WithTags("Rules Management").RequireAuthorization().RequireRateLimiting(SecurityConstants.Policies.StrictRateLimit);

            group.MapGet("/", async (IMediator mediator) =>
            {
                return await mediator.Send(new GetActiveRulesQuery()) switch
                {
                    { IsError: true } result => result.ToProblemDetails(),
                    var result => Results.Ok(result.Value)
                };

            });

            group.MapGet("/{ruleName}", async (string ruleName, ISender sender, CancellationToken ct) =>
            {
                var query = new GetRuleByNameQuery(ruleName);
                var result = await sender.Send(query, ct);

                return result is not null
                    ? Results.Ok(result)
                    : Results.NotFound(new { Message = $"Rule '{ruleName}' was not found." });
            });

            group.MapPut("/{ruleName}/toggle", async (string ruleName, [FromBody] ToggleRuleRequest request, IMediator mediator) =>
            {
                var command = new ToggleRuleStatusCommand(ruleName, request.IsActive);
                return await mediator.Send(command) switch
                {
                    { IsError: true } result => result.ToProblemDetails(),
                    _ => Results.NoContent()
                };
            });

            group.MapPost("/", async ([FromBody] CreateRuleRequest request, IMediator mediator) =>
            {
                var command = new CreateRuleCommand(request.RuleName, request.Description, request.Expression);

                return await mediator.Send(command) switch
                {
                    { IsError: true } result => result.ToProblemDetails(),

                    _ => Results.Created($"/api/rules/{request.RuleName}", null)
                };
            });


            group.MapPut("/{ruleName}/expression", async (string ruleName, [FromBody] UpdateExpressionRequest request, IMediator mediator) =>
            {
                var command = new UpdateRuleExpressionCommand(ruleName, request.NewExpression);
                return await mediator.Send(command) switch
                {
                    { IsError: true } result => result.ToProblemDetails(),
                    _ => Results.NoContent()
                };
            });

            group.MapPost("/cache/clear", async ([FromServices] IFraudRuleManager manager, CancellationToken ct) =>
            {
                try
                {
                    await manager.SynchronizeAllRulesAsync(ct);
                    return Results.Ok(new
                    {
                        Message = "Synchronized clear successful",
                        Timestamp = DateTime.UtcNow,
                        LayersReset = new[] { "RepositoryCache", "CompiledLambdas", "RuleState" }
                    });
                }
                catch (Exception)
                {

                    return Results.Problem("Failed to synchronize rule engine state.");
                }
            })
            .WithName("SynchronizeRules")
            .WithOpenApi();


            group.MapPut("/{name}/threshold", async (string name, UpdateThresholdRequest request, ISender mediator, CancellationToken ct) =>
            {

                var command = new UpdateRuleThresholdCommand(name, request.Parameters);

                var result = await mediator.Send(command, ct);

                return result.Match(
                    success => Results.Ok(new { Message = "Rule synchronized successfully", UpdatedAt = DateTime.UtcNow }),
                    errors => Results.BadRequest(errors)
                );
            })
            .WithName("UpdateRuleThreshold")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Updates JSON parameters for a specific rule and triggers a engine rules sync.";
                return operation;
            });

            group.MapPut("/{ruleName}", async (string ruleName, UpdateRuleRequest request, ISender sender, CancellationToken ct) =>
            {
                var command = new UpdateRuleCommand(
                    ruleName,
                    request.Description,
                    request.Expression,
                    request.Parameters,
                    request.IsActive);

                var result = await sender.Send(command, ct);

                if (result.IsError)
                {
                    var validationErrors = result.Errors
                        .GroupBy(e => e.Code)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());

                    return Results.ValidationProblem(validationErrors);
                }

                if (result.Value is null)
                {
                    return Results.NotFound(new { Message = $"Rule '{ruleName}' was not found and could not be updated." });
                }

                return Results.Ok(result.Value);
            });
        }
    }
}
