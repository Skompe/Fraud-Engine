using System;

namespace Capitec.FraudEngine.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid FraudFlagId { get; set; }
        public Guid? UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? UserRole { get; set; }
        public string? SourceSystem { get; set; }

        public static class Actions
        {
            public const string FlagCreated = "FLAG_CREATED";
            public const string FlagResolved = "FLAG_RESOLVED";
            public const string FlagEscalated = "FLAG_ESCALATED";
            public const string FlagAssigned = "FLAG_ASSIGNED";
            public const string ManualInvestigationIngested = "MANUAL_INVESTIGATION_INGESTED";
            public const string RuleApplied = "RULE_APPLIED";
            public const string TokenRefreshed = "TOKEN_REFRESHED";
            public const string UnauthorizedAccessAttempted = "UNAUTHORIZED_ACCESS_ATTEMPTED";
        }

        public static AuditLog Create(
            Guid fraudFlagId,
            Guid? userId,
            string action,
            string description,
            string? oldValue = null,
            string? newValue = null,
            string? userRole = null,
            string? sourceSystem = null)
        {
            return new AuditLog
            {
                FraudFlagId = fraudFlagId,
                UserId = userId,
                Action = action,
                Description = description,
                OldValue = oldValue,
                NewValue = newValue,
                UserRole = userRole,
                SourceSystem = sourceSystem,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}
