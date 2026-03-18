using ErrorOr;
using MassTransit;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Application.Features.Customers.IngestAsyncCustomers
{
    public record AsyncCustomerItem(string CustomerId,string FirstName,string LastName);
    public record IngestAsyncCustomersCommand(List<AsyncCustomerItem> Customers) : IRequest<ErrorOr<int>>;

    public class IngestAsyncCustomersHandler(IPublishEndpoint publishEndpoint): IRequestHandler<IngestAsyncCustomersCommand, ErrorOr<int>>
    {
        public async Task<ErrorOr<int>> Handle(IngestAsyncCustomersCommand request, CancellationToken ct)
        {
           
            await publishEndpoint.PublishBatch(request.Customers, ct);

            return request.Customers.Count;
        }
    }
}
