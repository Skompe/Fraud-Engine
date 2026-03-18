using Capitec.FraudEngine.Application.Behaviors;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Tests.Application.Behaviors
{
    public class ValidationBehaviorTests
    {
     
        public record DummyRequest(string Data) : IRequest<ErrorOr<string>>;

        [Fact]
        public async Task Handle_WithNoValidators_CallsNextDelegate()
        {
            // Arrange
            var validators = Enumerable.Empty<IValidator<DummyRequest>>();
            var behavior = new ValidationBehavior<DummyRequest, string>(validators);
            var request = new DummyRequest("ValidData");

            bool nextCalled = false;
            RequestHandlerDelegate<ErrorOr<string>> next  = delegate 
            {
                nextCalled = true;
                return Task.FromResult((ErrorOr<string>)"Success");
            };

            // Act
            var result = await behavior.Handle(request, next, CancellationToken.None);

            // Assert
            Assert.True(nextCalled);
            Assert.False(result.IsError);
            Assert.Equal("Success", result.Value);
        }

        [Fact]
        public async Task Handle_WithValidRequest_CallsNextDelegate()
        {
            // Arrange
            var request = new DummyRequest("ValidData");

            var validatorMock = new Mock<IValidator<DummyRequest>>();
            
            validatorMock.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<DummyRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var validators = new List<IValidator<DummyRequest>> { validatorMock.Object };
            var behavior = new ValidationBehavior<DummyRequest, string>(validators);

            bool nextCalled = false;
            RequestHandlerDelegate<ErrorOr<string>> next  = delegate
            {
                nextCalled = true;
                return Task.FromResult((ErrorOr<string>)"Success");
            };

            // Act
            var result = await behavior.Handle(request, next, CancellationToken.None);

            // Assert
            Assert.True(nextCalled);
            Assert.False(result.IsError);
            Assert.Equal("Success", result.Value);
        }

        [Fact]
        public async Task Handle_WithInvalidRequest_ShortCircuitsAndReturnsValidationErrors()
        {
            // Arrange
            var request = new DummyRequest("InvalidData");

            var validatorMock = new Mock<IValidator<DummyRequest>>();
            // Simulate
            validatorMock.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<DummyRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new[]
                {
                new ValidationFailure("Data", "Data must be valid.")
                }));

            var validators = new List<IValidator<DummyRequest>> { validatorMock.Object };
            var behavior = new ValidationBehavior<DummyRequest, string>(validators);

            bool nextCalled = false;
            RequestHandlerDelegate<ErrorOr<string>> next = delegate
            {
                nextCalled = true;
                return Task.FromResult((ErrorOr<string>)"Success");
            };

            // Act
            var result = await behavior.Handle(request, next, CancellationToken.None);

            // Assert
            Assert.False(nextCalled); 
            Assert.True(result.IsError);
            Assert.Single(result.Errors);

            // Verify 
            var error = result.FirstError;
            Assert.Equal(ErrorType.Validation, error.Type);
            Assert.Equal("Data", error.Code);
            Assert.Equal("Data must be valid.", error.Description);
        }
    }
}
