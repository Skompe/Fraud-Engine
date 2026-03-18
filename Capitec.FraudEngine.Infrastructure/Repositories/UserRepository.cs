using Capitec.FraudEngine.Domain.Abstractions.Data;
using Capitec.FraudEngine.Domain.Entities;
using Capitec.FraudEngine.Infrastructure.Persistence.Abstractions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Infrastructure.Repositories
{
    public class UserRepository(IFraudDbContext context): IUserRepository
    {
        public void Add(User user)
        {
            context.Users.Add(user);
        }

        public async Task<bool> ExistsAsync(string username, CancellationToken cancellationToken = default)
        {
            var normalizedUsername = username.ToLowerInvariant();

            return await context.Users.AnyAsync(u => u.Username == normalizedUsername, cancellationToken);
        }
        public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
        {
        
            return await context.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Username == username, ct);
        }

        public async Task<User?> GetByIdAsync(Guid userId, CancellationToken ct = default)
        {
            return await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
        }
    }
}
