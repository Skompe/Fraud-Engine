using Capitec.FraudEngine.API.Extensions;
using Capitec.FraudEngine.Application.Features.Transactions.CalculateCustomerVelocity;
using Capitec.FraudEngine.Application.Features.Transactions.GetTransactionById;
using Capitec.FraudEngine.Application.Features.Transactions.IngestAsyncTransaction;
using Capitec.FraudEngine.Application.Features.Transactions.IngestAsyncTransactions;
using Capitec.FraudEngine.Application.Features.Transactions.ProcessSyncTransaction;
using Capitec.FraudEngine.API.Constants;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Capitec.FraudEngine.API.Endpoints
{
    public static class TransactionEndpoints
    {
        public static void MapTransactionEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/transactions").WithTags("Transactions").RequireAuthorization().RequireRateLimiting(SecurityConstants.Policies.StrictRateLimit);

            group.MapPost("/sync", async ([FromBody] ProcessSyncTransactionCommand command, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(command, ct);
                return result.IsError ? result.ToProblemDetails() : Results.Ok(result.Value);
            }).RequireAuthorization(SecurityConstants.Policies.FraudWrite);

            group.MapPost("/async", async ([FromBody] IngestAsyncTransactionCommand command, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(command, ct);
                return result.IsError ? result.ToProblemDetails() : Results.Accepted();
            }).RequireAuthorization(SecurityConstants.Policies.FraudWrite);


            group.MapGet("/{id}", async (string id, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetTransactionByIdQuery(id), ct);
                return result.IsError ? result.ToProblemDetails() : Results.Ok(result.Value);
            }).RequireAuthorization(SecurityConstants.Policies.FraudRead);

            group.MapGet("/customer/{customerId}/velocity", async (string customerId, [FromQuery] int minutes, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetCustomerVelocityQuery(customerId, minutes), ct);
                return result.IsError ? result.ToProblemDetails() : Results.Ok(result.Value);
            }).RequireAuthorization(SecurityConstants.Policies.FraudRead);


            group.MapPost("/batch", async (IngestAsyncTransactionsCommand command, ISender mediator, CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(command, cancellationToken);

                if (result.IsError)
                {
                    return result.ToProblemDetails();
                }

                return Results.Accepted(string.Empty, new
                {
                    Message = $"Successfully queued {result.Value} transactions for asynchronous processing.",
                    Timestamp = DateTime.UtcNow
                });
            }).RequireAuthorization(SecurityConstants.Policies.FraudWrite);
        }
    }
}
