using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Domain.Entities
{
    public class RuleConfiguration
    {
        private RuleConfiguration() { }

        public RuleConfiguration(string ruleName, string description, string? expression, string? parameters = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(ruleName);
            ArgumentException.ThrowIfNullOrWhiteSpace(description);


            Id = Guid.NewGuid();
            RuleName = ruleName;
            Description = description;
            Expression = expression;
            Parameters = parameters; 
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public Guid Id { get; private set; }
        public string RuleName { get; private set; }
        public string Description { get; private set; }
        public string? Expression { get; private set; }
        public string? Parameters { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public void Toggle(bool isActive)
        {
            if (IsActive == isActive) return;
            IsActive = isActive;
            UpdatedAt = DateTime.UtcNow;
        }

        
        public void UpdateExpression(string newExpression)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(newExpression);
            Expression = newExpression;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateParameters(string newParameters)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(newParameters);
            Parameters = newParameters;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string description, string? expression, string? parameters, bool isActive)
        {
            Description = description;
            Expression = expression;
            Parameters = parameters;
            IsActive = isActive;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
