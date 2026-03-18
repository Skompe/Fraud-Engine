using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Domain.Abstractions.Rules
{
    public interface IFraudRuleManager
    {
        Task SynchronizeAllRulesAsync(CancellationToken ct = default);
        Task InitializeRulesAsync(CancellationToken ct = default);
    }
}
