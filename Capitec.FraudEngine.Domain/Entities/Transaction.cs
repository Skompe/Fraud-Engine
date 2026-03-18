using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Capitec.FraudEngine.Domain.Entities
{
    public class Transaction
    {  
        private Transaction() { }

        public Transaction(string transactionId, string customerId, decimal amount, string currency, DateTime timestamp)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(transactionId);
            ArgumentException.ThrowIfNullOrWhiteSpace(customerId);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
            ArgumentException.ThrowIfNullOrWhiteSpace(currency);

            Id = Guid.NewGuid(); 
            TransactionId = transactionId;
            CustomerId = customerId;
            Amount = amount;
            Currency = currency.ToUpperInvariant();
            Timestamp = timestamp;
        }


        public Guid Id { get; private set; }

        public string TransactionId { get; private set; }

        public string CustomerId { get; private set; }

        public decimal Amount { get; private set; }

        public string Currency { get; private set; }

        public DateTime Timestamp { get; private set; }
    }
}
