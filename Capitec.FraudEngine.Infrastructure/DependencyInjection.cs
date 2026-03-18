using Capitec.FraudEngine.Application.Abstractions.Authentication;
using Capitec.FraudEngine.Application.Abstractions.Caching;
using Capitec.FraudEngine.Domain.Abstractions.Data;
using Capitec.FraudEngine.Domain.Abstractions.Rules;
using Capitec.FraudEngine.Domain.Rules;
using Capitec.FraudEngine.Infrastructure.Authentication;
using Capitec.FraudEngine.Infrastructure.Managers;
using Capitec.FraudEngine.Infrastructure.Messaging;
using Capitec.FraudEngine.Infrastructure.Messaging.Consumer;
using Capitec.FraudEngine.Infrastructure.Persistence;
using Capitec.FraudEngine.Infrastructure.Persistence.Abstractions;
using Capitec.FraudEngine.Infrastructure.Repositories;
using Capitec.FraudEngine.Infrastructure.Rules;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {

            
            services.AddMemoryCache();

            
            services.AddDbContext<FraudDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), dbOptions =>
                {
                    dbOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
                }
                ));

            services.AddScoped<IFraudDbContext>(sp => sp.GetRequiredService<FraudDbContext>());
            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<FraudDbContext>());
            services.AddTransient<FraudDbSeeder>();
            services.AddScoped<VelocityRule>();
            services.AddScoped<IFraudRule>(sp => sp.GetRequiredService<VelocityRule>());
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IFraudFlagRepository, FraudFlagRepository>();
            services.AddScoped<IFraudRuleManager, FraudRuleManager>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddSingleton<IPasswordService, PasswordService>();
            services.AddScoped<IRuleCacheInvalidator, RuleCacheInvalidator>();
            services.AddScoped<RuleRepository>();
            services.AddScoped<IDynamicRuleEvaluator, DynamicRuleEvaluator>();
            services.AddScoped<IRuleRepository>(provider =>
            {
                var realRepo = provider.GetRequiredService<RuleRepository>();
                var cache = provider.GetRequiredService<IMemoryCache>();
                var evaluator = provider.GetRequiredService<IDynamicRuleEvaluator>();
                return new CachedRuleRepository(realRepo, cache,evaluator); 
            });

            
        
            services.AddMassTransit(x =>
            {
                
                x.AddConsumer<AsyncTransactionConsumer>();
                x.AddConsumer<AsyncInvestigationConsumer>();
                x.AddConsumer<AsyncCustomerConsumer>();
                services.AddScoped<ICustomerRepository, CustomerRepository>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(configuration["RabbitMQ:HostName"], h =>
                    {
                        h.Username(configuration["RabbitMQ:UserName"]!);
                        h.Password(configuration["RabbitMQ:Password"]!);
                    });

                    
                    cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

                    
                    cfg.ConfigureEndpoints(context);
                });
            });


            return services;
        }
    }
}
