using Capitec.FraudEngine.Domain.Abstractions.Data;
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

    internal class ResolveFraudFlagHandler(IFraudFlagRepository repository, IUnitOfWork unitOfWork): IRequestHandler<ResolveFraudFlagCommand, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(ResolveFraudFlagCommand request, CancellationToken ct)
        {
            var flag = await repository.GetByIdAsync(request.FlagId, ct);
            if (flag is null) return Error.NotFound("FraudFlag.NotFound", "The specified fraud flag does not exist.");

            flag.Resolve(request.ResolutionStatus, request.AnalystNotes);
            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
