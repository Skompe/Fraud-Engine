using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Threading.Tasks;
using Capitec.FraudEngine.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Capitec.FraudEngine.Tests.Fixtures
{
    public class FraudEngineWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("fraud_engine_test")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .Build();

        private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management-alpine")
            .WithUsername("test_rabbitmq")
            .WithPassword("test_password")
            .Build();

        public async Task InitializeAsync()
        {
            await _postgresContainer.StartAsync();
            await _rabbitMqContainer.StartAsync();
        }

        public new async Task DisposeAsync()
        {
            await _postgresContainer.StopAsync();
            await _rabbitMqContainer.StopAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.UseEnvironment("Production");

            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = _postgresContainer.GetConnectionString(),
                    ["RabbitMQ:HostName"] = "localhost",
                    ["RabbitMQ:Port"] = "5672",
                    ["RabbitMQ:UserName"] = "test_rabbitmq",
                    ["RabbitMQ:Password"] = "test_password",
                    ["Jwt:Key"] = "HyN0m9Le4QHHQds944iZYPB611M60k9MkUSUcWdB5Cc=",
                    ["Jwt:Issuer"] = "fraud-engine",
                    ["Jwt:Audience"] = "fraud-engine-api",
                    ["Jwt:ExpiryInMinutes"] = "60"
                });
            });

            builder.ConfigureServices(services =>
            { 
                var dbContextDescriptor = services.SingleOrDefault(d =>
                    d.ServiceType == typeof(DbContextOptions<FraudDbContext>));

                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }

                services.AddDbContext<FraudDbContext>(options =>
                {
                    options.UseNpgsql(_postgresContainer.GetConnectionString());
                });

                services.RemoveAll<IHostedService>();

                // Apply migrations
                using (var scope = services.BuildServiceProvider().CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<FraudDbContext>();
                    dbContext.Database.Migrate();
                }
            });
        }

        public new HttpClient CreateClient()
        {
            return base.CreateClient();
        }
    }
}
