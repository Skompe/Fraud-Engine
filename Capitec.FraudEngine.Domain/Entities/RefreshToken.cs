using System;

namespace Capitec.FraudEngine.Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? ReplacedByToken { get; set; }
        public int RotationCount { get; set; } = 0;
        public bool IsValid => DateTime.UtcNow <= ExpiresAt && !RevokedAt.HasValue;
        public bool IsNearingExpiration => DateTime.UtcNow.AddMinutes(5) >= ExpiresAt && IsValid;
        public void Revoke(string newToken)
        {
            RevokedAt = DateTime.UtcNow;
            ReplacedByToken = newToken;
        }

        public void IncrementRotation()
        {
            RotationCount++;
        }
    }
}
