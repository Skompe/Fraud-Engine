namespace Capitec.FraudEngine.Application.Constants
{
    public static class IdentityConstants
    {
        public static class Roles
        {
            public const string Admin = "Admin";
            public const string Analyst = "Analyst";
            public const string System = "System";
        }

        public static class Scopes
        {
            public const string FraudRead = "fraud.read";
            public const string FraudWrite = "fraud.write";
        }

        public static class Claims
        {
            public const string Scope = "scope";
        }

        public static class Tokens
        {
            public const string Bearer = "Bearer";
            public const int AccessTokenExpiresInSeconds = 3600;
        }

        public static class SourceSystems
        {
            public const string FraudEngineApi = "FraudEngine.API";
        }
    }
}