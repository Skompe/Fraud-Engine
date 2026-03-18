using Capitec.FraudEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Domain.Abstractions.Data
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task<RefreshToken?> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(Guid userId);
        Task AddAsync(RefreshToken token);
        Task UpdateAsync(RefreshToken token);
        Task DeleteAsync(Guid tokenId);
        Task RevokeAllByUserIdAsync(Guid userId, string newToken);
    }

    public interface IAuditLogRepository
    {
        Task AddAsync(AuditLog auditLog);
        Task<IEnumerable<AuditLog>> GetByFraudFlagIdAsync(Guid fraudFlagId);
        Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<AuditLog>> GetByActionAsync(string action);
    }
}
