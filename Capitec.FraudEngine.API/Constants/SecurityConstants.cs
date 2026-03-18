using Capitec.FraudEngine.Application.Constants;

namespace Capitec.FraudEngine.API.Constants
{
    public static class SecurityConstants
    {
        public static class Roles
        {
            public const string Admin = IdentityConstants.Roles.Admin;
            public const string Analyst = IdentityConstants.Roles.Analyst;
            public const string System = IdentityConstants.Roles.System;
        }

        public static class Policies
        {
            public const string StrictRateLimit = "strict";
        }

        public static class Swagger
        {
            public const string SchemeName = "Bearer";
            public const string Scheme = "bearer";
            public const string BearerFormat = "JWT";
            public const string TokenDescription = "Enter your JWT token";
        }

        public static class TestAuth
        {
            public const string Scheme = "TestScheme";
        }
    }
}
