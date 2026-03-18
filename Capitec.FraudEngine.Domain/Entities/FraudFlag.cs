using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Capitec.FraudEngine.Domain.Constants;

namespace Capitec.FraudEngine.Domain.Entities
{
    public class FraudFlag
    {
        private readonly List<string> _triggeredRules = new();

        private FraudFlag() { }

        public FraudFlag(string transactionId, string customerId, string status,string reason, string severity, string source, IEnumerable<string> triggeredRules)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(transactionId);
            ArgumentException.ThrowIfNullOrWhiteSpace(customerId);
            ArgumentException.ThrowIfNullOrWhiteSpace(severity);

            Id = Guid.NewGuid();
            TransactionId = transactionId;
            CustomerId = customerId;
            Status = status ?? DomainConstants.FraudStatus.Pending;
            Severity = severity;
            this.Reason = reason;
            Source = source;
            CreatedAt = DateTime.UtcNow;

            if (triggeredRules != null)
            {
                _triggeredRules.AddRange(triggeredRules);
            }
        }

        public Guid Id { get; private set; }
        public string TransactionId { get; private set; }
        public string CustomerId { get; private set; }
        public string Status { get; private set; }
        public string Severity { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ResolvedAt { get; private set; }
        public string? AnalystNotes { get; private set; }
        public string Source { get; private set; }
        public string Reason { get; private set; }


        public IReadOnlyCollection<string> TriggeredRules => _triggeredRules.AsReadOnly();

        
        public void Resolve(string resolutionStatus, string analystNotes)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(resolutionStatus);

            if (!string.Equals(Status, DomainConstants.FraudStatus.Pending, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Only pending flags can be resolved.");
            }

            if (resolutionStatus != DomainConstants.FraudStatus.FalsePositive && resolutionStatus != DomainConstants.FraudStatus.ConfirmedFraud)
            {
                throw new ArgumentException("Invalid resolution status.");
            }

            Status = resolutionStatus;
            AnalystNotes = analystNotes;
            ResolvedAt = DateTime.UtcNow;
        }
    }
}
