namespace Capitec.FraudEngine.Application.Constants
{
    public static class ErrorCodes
    {
        public static class Transactions
        {
            public const string DuplicateCode = "Transaction.Duplicate";
            public const string DuplicateProcessedMessage = "Transaction has already been processed.";
            public const string DuplicateIngestedMessage = "Transaction has already been ingested.";
        }

        public static class Rules
        {
            public const string NotFoundCode = "Rule.NotFound";
            public const string NotFoundMessageTemplate = "Rule '{0}' not found.";
            public const string DoesNotExistMessage = "The specified rule does not exist.";
            public const string InvalidParametersCode = "Rule.InvalidParameters";
            public const string InvalidParametersMessage = "The parameters provided are not valid JSON.";
        }

        public static class Identity
        {
            public const string InvalidRefreshTokenCode = "INVALID_REFRESH_TOKEN";
            public const string UserNotFoundCode = "USER_NOT_FOUND";
        }
    }
}
