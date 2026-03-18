using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Domain.Entities
{
    public class Customer
    {
        public Customer() { } 

        public Customer(string customerId, string firstName, string lastName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(customerId);
            ArgumentException.ThrowIfNullOrWhiteSpace(firstName);

            CustomerId = customerId;
            FirstName = firstName;
            LastName = lastName;
            RiskCategory = "Standard";
            CreatedAt = DateTime.UtcNow;
        }

        public string CustomerId { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string RiskCategory { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public void UpdateRiskCategory(string category)
        {
            RiskCategory = category;
        }
    }
}
