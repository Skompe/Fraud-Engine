**Capitec Fraud Engine**

The Capitec Fraud Engine is a fraud detection system built with .NET 9. This engine evaluates transactions against dynamic and built-in rule sets in real-time to identify and flag fraudulent activity.

**Architecture Overview:**

This solution implements CLEAN Architecture combined with DDD to ensure core business rules remain completely decoupled from infrastructure. 
Furthermore, the Application layer utilizes the CQRS pattern (via MediatR) and is structured using Feature Folders. By vertically slicing the application layer by business aggregate (e.g., Transactions, Rules) rather than technical concern, we keep related commands, queries, and handlers highly cohesive.



**Solution Structure:**

Capitec.FraudEngine.Domain: The pure core. Contains enterprise-wide logic, DDD Entities (Transaction, Customer, FraudFlag), and repository abstractions. No external dependencies.

Capitec.FraudEngine.Application: The business use cases. Organized by Feature Folders containing MediatR pipelines, FluentValidation rules, and the DynamicRuleEvaluator.

     Features
        |
        --->  Customers (e.g., GetCustomersPaged)

        --->  Investigations (e.g., ResolveFraudFlag)


Capitec.FraudEngine.Infrastructure: Contains implementation details, including Entity Framework Core DbContexts, PostgreSQL repositories, and messaging infrastructure.

Capitec.FraudEngine.API: The presentation layer utilizing ASP.NET Core Minimal APIs, secured with JWT authentication.

Capitec.FraudEngine.Tests: A comprehensive, test suite enforcing system integrity.


**Prerequisites:**

.NET 9 SDK
PostgreSQL Database
RabbitMQ
Podman or Docker. 
Postman or similar API testing tool.


**Getting Started:**

1. Compile and run the application:

    The solution relies on PostgreSQL and RabbitMQ. These are fully containerized and can be spun up using Podman/Docker Compose.

    ** Open your terminal at the Capitec.FraudEngine.API project and run:

        podman compose up -d --build or
        docker-compose up -d --build   

    The containers will spin up, automatically apply database migrations, and seed the initial test data (sample rules, users, and  flags).
    If you need to completely wipe your local database and start fresh, run podman compose down -v before build it back up


2. Access the API:
    
            
    The API will be available at http://localhost:8080/scalar/v1. You can use Postman or any API testing tool to interact with the endpoints. 
    The API is secured with JWT authentication, so you will need to obtain a token by logging in with the seeded user credentials (e.g., username: admin@capitec.co.za, password: Capitec@2026!). 

  
3. Test the API:

    Use Postman or a similar tool to test the API endpoints. You can perform operations such as retrieving transactions, creating new transactions, and resolving fraud flags. To make interacting with the API as frictionless as possible, a pre-configured Postman collection is included in the root of solution (Capitec Fraud Engine.postman_collection.json).


    
   Should you need anything please reach out to me.
   Please note that Batch processes are dependent on the queuing, those wont work when running the solution in debugging.
