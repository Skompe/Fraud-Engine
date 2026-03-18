using Capitec.FraudEngine.Domain.Abstractions.Data;
using Capitec.FraudEngine.Domain.Entities;
using Capitec.FraudEngine.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Infrastructure.Repositories
{

    public class RefreshTokenRepository(FraudDbContext context) : IRefreshTokenRepository
    {
        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token && !rt.RevokedAt.HasValue);
        }

        public async Task<RefreshToken?> GetByUserIdAsync(Guid userId)
        {
            return await context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.IsValid)
                .OrderByDescending(rt => rt.IssuedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(Guid userId)
        {
            return await context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.IsValid)
                .OrderByDescending(rt => rt.IssuedAt)
                .ToListAsync();
        }

        public async Task AddAsync(RefreshToken token)
        {
            await context.RefreshTokens.AddAsync(token);
        }

        public async Task UpdateAsync(RefreshToken token)
        {
            context.RefreshTokens.Update(token);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid tokenId)
        {
            var token = await context.RefreshTokens.FindAsync(tokenId);
            if (token != null)
            {
                context.RefreshTokens.Remove(token);
            }
        }

        public async Task RevokeAllByUserIdAsync(Guid userId, string newToken)
        {
            var tokens = await context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.RevokedAt.HasValue)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.Revoke(newToken);
                context.RefreshTokens.Update(token);
            }
        }
    }


    public class AuditLogRepository(FraudDbContext context) : IAuditLogRepository
    {
        public async Task AddAsync(AuditLog auditLog)
        {
            await context.AuditLogs.AddAsync(auditLog);
        }

        public async Task<IEnumerable<AuditLog>> GetByFraudFlagIdAsync(Guid fraudFlagId)
        {
            return await context.AuditLogs
                .Where(al => al.FraudFlagId == fraudFlagId)
                .OrderByDescending(al => al.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId)
        {
            return await context.AuditLogs
                .Where(al => al.UserId == userId)
                .OrderByDescending(al => al.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await context.AuditLogs
                .Where(al => al.Timestamp >= startDate && al.Timestamp <= endDate)
                .OrderByDescending(al => al.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByActionAsync(string action)
        {
            return await context.AuditLogs
                .Where(al => al.Action == action)
                .OrderByDescending(al => al.Timestamp)
                .ToListAsync();
        }
    }
}
