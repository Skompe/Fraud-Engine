using Capitec.FraudEngine.Domain.Entities;

namespace Capitec.FraudEngine.Application.Abstractions.Authentication
{
    public interface IPasswordService
    {
        string HashPassword(string password);
        bool VerifyPassword(User user, string providedPassword);
    }
}
