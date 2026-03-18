using Capitec.FraudEngine.Domain.Abstractions.Data;
using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Application.Features.Customers.GetCustomersPaged
{
    public record PagedCustomerResponse(IEnumerable<CustomerDto> Items, int TotalCount, int Page, int PageSize);
    public record CustomerDto(string CustomerId, string FirstName, string LastName, DateTime CreatedAt);

    public record GetCustomersPagedQuery(int Page, int PageSize) : IRequest<ErrorOr<PagedCustomerResponse>>;

    public class GetCustomersPagedHandler(ICustomerRepository repo) : IRequestHandler<GetCustomersPagedQuery, ErrorOr<PagedCustomerResponse>>
    {
        public async Task<ErrorOr<PagedCustomerResponse>> Handle(GetCustomersPagedQuery request, CancellationToken ct)
        {
            var (items, total) = await repo.GetPagedAsync(request.Page, request.PageSize, ct);

            var dtos = items.Select(c => new CustomerDto(c.CustomerId, c.FirstName, c.LastName, c.CreatedAt));

            return new PagedCustomerResponse(dtos, total, request.Page, request.PageSize);
        }
    }
}
