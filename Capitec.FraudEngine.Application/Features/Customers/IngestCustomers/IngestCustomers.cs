using Capitec.FraudEngine.Domain.Abstractions.Data;
using Capitec.FraudEngine.Domain.Entities;
using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Application.Features.Customers.IngestCustomers
{
    public record CustomerItem(string CustomerId, string FirstName, string LastName);
    public record IngestCustomersCommand(List<CustomerItem> Customers) : IRequest<ErrorOr<int>>;

    public class IngestCustomersHandler(ICustomerRepository repo) : IRequestHandler<IngestCustomersCommand, ErrorOr<int>>
    {
        public async Task<ErrorOr<int>> Handle(IngestCustomersCommand request, CancellationToken ct)
        {
            var entities = request.Customers.Select(c => new Customer(c.CustomerId,c.FirstName,c.LastName));

            await repo.AddBatchAsync(entities, ct);
            return entities.Count();
        }
    }
}
