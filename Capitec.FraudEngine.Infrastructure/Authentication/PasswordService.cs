using Capitec.FraudEngine.Application.Abstractions.Authentication;
using Capitec.FraudEngine.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Capitec.FraudEngine.Infrastructure.Authentication
{
    public class PasswordService : IPasswordService
    {
        private readonly PasswordHasher<User> _passwordHasher = new();

        public string HashPassword(string password)
        {
            return _passwordHasher.HashPassword(null!, password);
        }

        public bool VerifyPassword(User user, string providedPassword)
        {
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, providedPassword);
            return result != PasswordVerificationResult.Failed;
        }
    }
}
