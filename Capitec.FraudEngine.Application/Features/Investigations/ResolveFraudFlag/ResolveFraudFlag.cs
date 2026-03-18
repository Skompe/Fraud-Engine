using Capitec.FraudEngine.Domain.Abstractions.Data;
using Capitec.FraudEngine.Domain.Entities;
using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Application.Features.Investigations.ResolveFraudFlag
{
    public record ResolveFraudFlagCommand(Guid FlagId, string ResolutionStatus, string AnalystNotes) : IRequest<ErrorOr<Success>>;

    public class ResolveFraudFlagHandler(
        IFraudFlagRepository repository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork): IRequestHandler<ResolveFraudFlagCommand, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(ResolveFraudFlagCommand request, CancellationToken ct)
        {
            var flag = await repository.GetByIdAsync(request.FlagId, ct);
            if (flag is null) return Error.NotFound("FraudFlag.NotFound", "The specified fraud flag does not exist.");

            var previousStatus = flag.Status;
            try
            {
                flag.Resolve(request.ResolutionStatus, request.AnalystNotes);
            }
            catch (InvalidOperationException ex)
            {
                return Error.Conflict("FraudFlag.InvalidState", ex.Message);
            }
            catch (ArgumentException ex)
            {
                return Error.Validation("FraudFlag.InvalidResolution", ex.Message);
            }

            var auditLog = AuditLog.Create(
                fraudFlagId: flag.Id,
                userId: null,
                action: AuditLog.Actions.FlagResolved,
                description: "Fraud flag resolution updated.",
                oldValue: previousStatus,
                newValue: request.ResolutionStatus,
                sourceSystem: "FraudEngine");

            await auditLogRepository.AddAsync(auditLog);
            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
