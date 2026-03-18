using Capitec.FraudEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Domain.Abstractions.Data
{
    public interface IUserRepository
    {
        void Add(User user);
        Task<bool> ExistsAsync(string username, CancellationToken cancellationToken = default);
        Task<User?> GetByUsernameAsync(string username, CancellationToken ct);
        Task<User?> GetByIdAsync(Guid userId, CancellationToken ct = default);
    }
}
