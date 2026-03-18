namespace Capitec.FraudEngine.Domain.Constants
{
    public static class DomainConstants
    {
        public static class FraudStatus
        {
            public const string Clean = "CLEAN";
            public const string Flagged = "FLAGGED";
            public const string Pending = "PENDING";
            public const string FalsePositive = "FALSE_POSITIVE";
            public const string ConfirmedFraud = "CONFIRMED_FRAUD";
        }

        public static class Severity
        {
            public const string Low = "LOW";
            public const string Medium = "MEDIUM";
            public const string High = "HIGH";
            public const string Critical = "CRITICAL";

            public static readonly string[] All =
            {
                Low,
                Medium,
                High,
                Critical
            };
        }

        public static class Source
        {
            public const string RuleEngine = "RuleEngine";
        }
    }
}
