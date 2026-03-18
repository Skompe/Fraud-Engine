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

            group.MapGet("/pending", async (ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetPendingInvestigationsQuery(), ct);
                return result.IsError ? result.ToProblemDetails() : Results.Ok(result.Value);
            }).RequireAuthorization(SecurityConstants.Policies.FraudRead);

            group.MapGet("/customer/{customerId}", async (string customerId, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetCustomerFlagsQuery(customerId), ct);
                return result.IsError ? result.ToProblemDetails() : Results.Ok(result.Value);
            }).RequireAuthorization(SecurityConstants.Policies.FraudRead);

            group.MapPost("/{flagId}/resolve", async (Guid flagId, [FromBody] ResolveFraudFlagRequest request, ISender sender, CancellationToken ct) =>
            {
                var command = new ResolveFraudFlagCommand(flagId, request.ResolutionStatus, request.AnalystNotes);
                var result = await sender.Send(command, ct);
                return result.IsError ? result.ToProblemDetails() : Results.NoContent();
            }).RequireAuthorization(SecurityConstants.Policies.FraudWrite);

            group.MapPost("/batch", async (List<IngestInvestigationRequest> requests, ISender sender, CancellationToken ct) =>
            {
                var items = requests.Select(r => new InvestigationItem(r.TransactionId, r.CustomerId, r.Source, r.Status, r.Reason, r.Severity,r.Rules)).ToList();

                var command = new IngestInvestigationsCommand(items);

                var result = await sender.Send(command, ct);

                if (result.IsError)
                {
                    return result.ToProblemDetails();
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
