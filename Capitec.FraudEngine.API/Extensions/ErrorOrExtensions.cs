using ErrorOr;

namespace Capitec.FraudEngine.API.Extensions
{
    public static class ErrorOrExtensions
    {
        public static IResult ToProblemDetails(this IErrorOr errorOr)
        {
            if (!errorOr.IsError) return Results.Ok();

            var firstError = errorOr.Errors!.FirstOrDefault();

            return firstError.Type switch
            {
                ErrorType.Validation => Results.BadRequest(new { Errors = errorOr.Errors!.Select(e => new { e.Code, e.Description }) }),
                ErrorType.NotFound => Results.NotFound(new { firstError.Code, firstError.Description }),
                ErrorType.Conflict => Results.Conflict(new { firstError.Code, firstError.Description }),
                ErrorType.Unauthorized => Results.Unauthorized(),
                _ => Results.InternalServerError(new { firstError.Code, firstError.Description })
            };
        }
    }
}
