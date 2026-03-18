using Capitec.FraudEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Domain.Abstractions.Data
{
    public interface IFraudFlagRepository
    {
        Task<FraudFlag?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<List<FraudFlag>> GetPendingFlagsAsync(CancellationToken ct);
        Task<List<FraudFlag>> GetByCustomerIdAsync(string customerId, CancellationToken ct);
        void Add(FraudFlag flag);
    }
}
