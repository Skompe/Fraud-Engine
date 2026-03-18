using Capitec.FraudEngine.Domain.Abstractions.Data;
using Capitec.FraudEngine.Domain.Abstractions.Rules;
using Capitec.FraudEngine.Domain.Constants;
using Capitec.FraudEngine.Domain.Entities;
using ErrorOr;
using FluentValidation;
using MediatR;
using Capitec.FraudEngine.Application.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Error = ErrorOr.Error;

namespace Capitec.FraudEngine.Application.Features.Transactions.ProcessSyncTransaction
{
    public record ProcessSyncTransactionCommand(string TransactionId, string CustomerId, decimal Amount, string Currency) : IRequest<ErrorOr<TransactionResponse>>;
    public record TransactionResponse(string TransactionId, string Status, List<string> Rules);

    public class ProcessSyncTransactionHandler(
        ITransactionRepository repository,
        IFraudFlagRepository flagRepository,
        IRuleRepository _ruleRepository,
        IUnitOfWork unitOfWork,
        IDynamicRuleEvaluator dynamicRuleEval,
        IEnumerable<IFraudRule> builtInRules): IRequestHandler<ProcessSyncTransactionCommand, ErrorOr<TransactionResponse>>
    {
        public async Task<ErrorOr<TransactionResponse>> Handle(ProcessSyncTransactionCommand request, CancellationToken ct)
        {
            if (await repository.ExistsAsync(request.TransactionId, ct))
                return Error.Conflict(ErrorCodes.Transactions.DuplicateCode, ErrorCodes.Transactions.DuplicateProcessedMessage);

            var tx = new Transaction(request.TransactionId, request.CustomerId, request.Amount, request.Currency, DateTime.UtcNow);
            var activeRules = await _ruleRepository.GetActiveRulesAsync(ct);
            var dynamicEvalTask = dynamicRuleEval.EvaluateAsync(tx, activeRules, ct);
            var builtInTasks = builtInRules.Select(r => r.EvaluateAsync(tx, ct));
            var builtInEvalTask = Task.WhenAll(builtInTasks);

            await Task.WhenAll(dynamicEvalTask, builtInEvalTask);

            var dynamicTriggers = dynamicEvalTask.Result;
            var builtInTriggers = builtInEvalTask.Result.Where(r => r is not null).Cast<string>();
            var allTriggeredRules = dynamicTriggers.Concat(builtInTriggers).ToList();

            await repository.AddAsync(tx, ct);

            if (allTriggeredRules.Count != 0)
            {
                var severity = allTriggeredRules.Count > 1 ? DomainConstants.Severity.High : DomainConstants.Severity.Medium;
                var flag = new FraudFlag(
                                    tx.TransactionId,
                                    tx.CustomerId,
                                    DomainConstants.FraudStatus.Pending,
                                    $"Triggered {allTriggeredRules.Count} fraud rules",
                                    severity,
                                    DomainConstants.Source.RuleEngine,
                                    allTriggeredRules
                                   );
                flagRepository.Add(flag);
            }

            await unitOfWork.SaveChangesAsync(ct);
            return new TransactionResponse(tx.TransactionId, allTriggeredRules.Count > 0 ? DomainConstants.FraudStatus.Flagged : DomainConstants.FraudStatus.Clean, allTriggeredRules);
        }
    }
}
