using Capitec.FraudEngine.Application.Features.Customers.GetCustomersPaged;
using Capitec.FraudEngine.Application.Features.Customers.IngestAsyncCustomers;
using Capitec.FraudEngine.Application.Features.Customers.IngestCustomers;
using MediatR;

namespace Capitec.FraudEngine.API.Endpoints
{
    public record IngestCustomerRequest(string CustomerId, string FirstName, string LastName);

    public static class CustomerEndpoints
    {
        public static void MapCustomerEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/customers").WithTags("Customers").RequireAuthorization();

            group.MapPost("/batch-no-pubsub", async (List<IngestCustomerRequest> requests, ISender sender) =>
            {
                var items = requests.Select(r => new CustomerItem(r.CustomerId, r.FirstName, r.LastName)).ToList();
                var result = await sender.Send(new IngestCustomersCommand(items));

                return result.IsError ? Results.BadRequest(result.Errors) : Results.Ok(result.Value);
            });

            group.MapGet("/", async (int page, int pageSize, ISender sender) =>
            {
                var result = await sender.Send(new GetCustomersPagedQuery(page, pageSize));
                return result.IsError ? Results.BadRequest(result.Errors) : Results.Ok(result.Value);
            });

            group.MapPost("/batch", async (IngestAsyncCustomersCommand command, ISender sender) =>
            {
                var result = await sender.Send(command);

                return result.IsError
                    ? Results.BadRequest(result.Errors)
                    : Results.Accepted(string.Empty, new
                    {
                        Message = $"Queued {result.Value} customers for processing.",
                        Timestamp = DateTime.UtcNow
                    });
            });
        }
    }
}
