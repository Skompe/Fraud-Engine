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

            group.MapPost("/sync", async ([FromBody] ProcessSyncTransactionCommand command, IMediator mediator) =>
            {
                return await mediator.Send(command) switch
                {
                    { IsError: true } result => result.ToProblemDetails(),
                    var result => Results.Ok(result.Value)
                };
            });

            group.MapPost("/async", async ([FromBody] IngestAsyncTransactionCommand command, IMediator mediator) =>
            {
                return await mediator.Send(command) switch
                {
                    { IsError: true } result => result.ToProblemDetails(),
                    _ => Results.Accepted()
                };

            });


            group.MapGet("/{id}", async (string id, IMediator mediator) =>
            {
                return await mediator.Send(new GetTransactionByIdQuery(id)) switch
                {
                    { IsError: true } result => result.ToProblemDetails(),
                    var result => Results.Ok(result.Value)
                };
            });

            group.MapGet("/customer/{customerId}/velocity", async (string customerId, [FromQuery] int minutes, IMediator mediator) =>
            {
                return await mediator.Send(new GetCustomerVelocityQuery(customerId, minutes)) switch
                {
                    { IsError: true } result => result.ToProblemDetails(),
                    var result => Results.Ok(result.Value)
                };


            });


            group.MapPost("/batch", async (IngestAsyncTransactionsCommand command, ISender mediator, CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(command, cancellationToken);

                if (result.IsError)
                {
                    return Results.BadRequest(result.Errors);
                }

                
                return Results.Accepted(string.Empty, new
                {
                    Message = $"Successfully queued {result.Value} transactions for asynchronous processing.",
                    Timestamp = DateTime.UtcNow
                });
            });
        }
    }
}
