using Capitec.FraudEngine.API.Extensions;
using Capitec.FraudEngine.Application.Features.Investigations.GetCustomerFlags;
using Capitec.FraudEngine.Application.Features.Investigations.GetPendingInvestigations;
using Capitec.FraudEngine.Application.Features.Investigations.IngestInvestigations;
using Capitec.FraudEngine.Application.Features.Investigations.ResolveFraudFlag;
using Capitec.FraudEngine.API.Constants;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Capitec.FraudEngine.API.Endpoints
{
    public record ResolveFraudFlagRequest(string ResolutionStatus, string AnalystNotes);
    public record IngestInvestigationRequest(string TransactionId, string CustomerId, string Source, string Status, string Reason, string Severity,IEnumerable<string> Rules);
    public static class InvestigationEndpoints
    {
        public static void MapInvestigationEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/investigations").WithTags("Investigations").RequireAuthorization().RequireRateLimiting(SecurityConstants.Policies.StrictRateLimit);

            group.MapGet("/pending", async (IMediator mediator) =>
            {
                return await mediator.Send(new GetPendingInvestigationsQuery()) switch
                {
                    { IsError: true } result => result.ToProblemDetails(),
                    var result => Results.Ok(result.Value)
                };
            }).RequireAuthorization(SecurityConstants.Policies.FraudRead);

            group.MapGet("/customer/{customerId}", async (string customerId, IMediator mediator) =>
            {
                return await mediator.Send(new GetCustomerFlagsQuery(customerId)) switch
                {
                    { IsError: true } result => result.ToProblemDetails(),
                    var result => Results.Ok(result.Value)
                };
            }).RequireAuthorization(SecurityConstants.Policies.FraudRead);

            group.MapPost("/{flagId}/resolve", async (Guid flagId, [FromBody] ResolveFraudFlagRequest request, IMediator mediator) =>
            {
                var command = new ResolveFraudFlagCommand(flagId, request.ResolutionStatus, request.AnalystNotes);
                return await mediator.Send(command) switch
                {
                    { IsError: true } result => result.ToProblemDetails(),
                    _ => Results.NoContent()
                };
            }).RequireAuthorization(SecurityConstants.Policies.FraudWrite);

            group.MapPost("/batch", async (List<IngestInvestigationRequest> requests, ISender sender, CancellationToken ct) =>
            {
                
                var items = requests.Select(r => new InvestigationItem(r.TransactionId, r.CustomerId, r.Source, r.Status, r.Reason, r.Severity,r.Rules)).ToList();

                var command = new IngestInvestigationsCommand(items);

                var result = await sender.Send(command, ct);

               
                if (result.IsError)
                {
                    var validationErrors = result.Errors
                        .GroupBy(e => e.Code)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
                    return Results.ValidationProblem(validationErrors);
                }

                return Results.Created("/investigations", new
                {
                    Message = $"Successfully ingested {result.Value} investigations.",
                    ProcessedItems = result.Value
                });
            }).RequireAuthorization(SecurityConstants.Policies.FraudWrite);
        }
    }
}
