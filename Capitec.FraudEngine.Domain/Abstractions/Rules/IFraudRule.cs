using Capitec.FraudEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Domain.Abstractions.Rules
{
    public interface IFraudRule
    {
        string RuleKey { get; }
        Task<string?> EvaluateAsync(Transaction transaction, CancellationToken ct = default);
    }
}
