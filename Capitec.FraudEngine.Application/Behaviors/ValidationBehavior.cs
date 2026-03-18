using ErrorOr;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Application.Behaviors
{
    public class ValidationBehavior<TRequest, TValue>(IEnumerable<IValidator<TRequest>> validators): IPipelineBehavior<TRequest, ErrorOr<TValue>>
        where TRequest : IRequest<ErrorOr<TValue>>
    {
        public async Task<ErrorOr<TValue>> Handle(TRequest request, RequestHandlerDelegate<ErrorOr<TValue>> next, CancellationToken cancellationToken)
        {
            if (!validators.Any())
            {
                return await next();
            }

            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            // If there are no errors, proceed to the handler
            if (failures.Count == 0)
            {
                return await next();
            }

            // Map FluentValidation failuress to ErrorOr Validation Errors
            var errors = failures
                .Select(validationFailure => Error.Validation(
                    code: validationFailure.PropertyName,
                    description: validationFailure.ErrorMessage))
                .ToList();

            return errors;
        }
    }
}
