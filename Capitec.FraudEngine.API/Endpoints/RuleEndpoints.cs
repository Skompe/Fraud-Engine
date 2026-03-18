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

            group.MapGet("/", async (ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetActiveRulesQuery(), ct);
                return result.IsError ? result.ToProblemDetails() : Results.Ok(result.Value);
            }).RequireAuthorization(SecurityConstants.Policies.FraudRead);

            group.MapGet("/{ruleName}", async (string ruleName, ISender sender, CancellationToken ct) =>
            {
                var query = new GetRuleByNameQuery(ruleName);
                var result = await sender.Send(query, ct);

                return result is not null
                    ? Results.Ok(result)
                    : Results.NotFound(new { Message = $"Rule '{ruleName}' was not found." });
            }).RequireAuthorization(SecurityConstants.Policies.FraudRead);

            group.MapPut("/{ruleName}/toggle", async (string ruleName, [FromBody] ToggleRuleRequest request, ISender sender, CancellationToken ct) =>
            {
                var command = new ToggleRuleStatusCommand(ruleName, request.IsActive);
                var result = await sender.Send(command, ct);
                return result.IsError ? result.ToProblemDetails() : Results.NoContent();
            }).RequireAuthorization(SecurityConstants.Policies.FraudWrite);

            group.MapPost("/", async ([FromBody] CreateRuleRequest request, ISender sender, CancellationToken ct) =>
            {
                var command = new CreateRuleCommand(request.RuleName, request.Description, request.Expression);
                var result = await sender.Send(command, ct);
                return result.IsError
                    ? result.ToProblemDetails()
                    : Results.Created($"/api/rules/{request.RuleName}", null);
            }).RequireAuthorization(SecurityConstants.Policies.FraudWrite);


            group.MapPut("/{ruleName}/expression", async (string ruleName, [FromBody] UpdateExpressionRequest request, ISender sender, CancellationToken ct) =>
            {
                var command = new UpdateRuleExpressionCommand(ruleName, request.NewExpression);
                var result = await sender.Send(command, ct);
                return result.IsError ? result.ToProblemDetails() : Results.NoContent();
            }).RequireAuthorization(SecurityConstants.Policies.FraudWrite);

            group.MapPost("/cache/clear", async ([FromServices] IFraudRuleManager manager, CancellationToken ct) =>
            {
                try
                {
                    await manager.SynchronizeAllRulesAsync(ct);
                    return Results.Ok(new
                    {
                        Message = "Rule engine synchronization completed successfully.",
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
            .RequireAuthorization(SecurityConstants.Policies.FraudWrite);


            group.MapPut("/{name}/threshold", async (string name, UpdateThresholdRequest request, ISender mediator, CancellationToken ct) =>
            {

                var command = new UpdateRuleThresholdCommand(name, request.Parameters);

                var result = await mediator.Send(command, ct);

                return result.IsError
                    ? result.ToProblemDetails()
                    : Results.Ok(new { Message = "Rule updated and synchronized successfully.", UpdatedAt = DateTime.UtcNow });
            })
            .WithName("UpdateRuleThreshold")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Update JSON parameters for a rule and trigger rule engine synchronization.";
                return operation;
            })
            .RequireAuthorization(SecurityConstants.Policies.FraudWrite);

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
            }).RequireAuthorization(SecurityConstants.Policies.FraudWrite);
        }
    }
}
