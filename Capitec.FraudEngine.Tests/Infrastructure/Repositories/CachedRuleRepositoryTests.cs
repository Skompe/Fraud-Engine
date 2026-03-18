using Capitec.FraudEngine.Application.Constants;
using Capitec.FraudEngine.Domain.Abstractions.Rules;
using Capitec.FraudEngine.Domain.Entities;
using Capitec.FraudEngine.Infrastructure.Persistence;
using Capitec.FraudEngine.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace Capitec.FraudEngine.Tests.Infrastructure.Repositories;

public class CachedRuleRepositoryTests : IDisposable
{
    private readonly FraudDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly Mock<IDynamicRuleEvaluator> _evaluatorMock = new();
    private readonly CachedRuleRepository _sut;

    public CachedRuleRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<FraudDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

        _db = new FraudDbContext(options);
        _cache = new MemoryCache(new MemoryCacheOptions());

        var innerRepo = new RuleRepository(_db);
        _sut = new CachedRuleRepository(innerRepo, _cache, _evaluatorMock.Object);
    }

    public void Dispose()
    {
        _db.Dispose();
        _cache.Dispose();
    }

    private static RuleConfiguration ActiveRule(string name) =>
        new(name, $"Description for {name}", expression: null);

    [Fact]
    public async Task GetActiveRulesAsync_SecondCall_ReturnsCachedResult()
    {
        // Arrange
        var rule = ActiveRule("TestRule");
        _db.RuleConfigurations.Add(rule);
        await _db.SaveChangesAsync();

        // Act
        var first = await _sut.GetActiveRulesAsync(CancellationToken.None);

        _db.RuleConfigurations.Remove(rule);
        await _db.SaveChangesAsync();

        var second = await _sut.GetActiveRulesAsync(CancellationToken.None);

        // Assert
        Assert.Single(first);
        Assert.Single(second); 
    }

    [Fact]
    public async Task Add_InvalidatesCache()
    {
        // Arrange
        var rule = ActiveRule("Rule-A");
        _db.RuleConfigurations.Add(rule);
        await _db.SaveChangesAsync();
        await _sut.GetActiveRulesAsync(CancellationToken.None);

        // Verify 
        Assert.True(_cache.TryGetValue(CacheKeys.Rules.ActiveRules, out _));

        // Act
        var newRule = ActiveRule("Rule-B");
        _db.RuleConfigurations.Add(newRule);
        await _db.SaveChangesAsync();
        _sut.Add(newRule); 

        // Assert
        Assert.False(_cache.TryGetValue(CacheKeys.Rules.ActiveRules, out _));
        _evaluatorMock.Verify(e => e.ClearCache(), Times.Once);
    }

    [Fact]
    public async Task AddAsync_InvalidatesCache()
    {
        // Arrange
        var rule = ActiveRule("Rule-X");
        _db.RuleConfigurations.Add(rule);
        await _db.SaveChangesAsync();
        await _sut.GetActiveRulesAsync(CancellationToken.None);

        Assert.True(_cache.TryGetValue(CacheKeys.Rules.ActiveRules, out _));

        // Act
        var newRule = ActiveRule("Rule-Y");
        _db.RuleConfigurations.Add(newRule);
        await _db.SaveChangesAsync();
        await _sut.AddAsync(newRule, CancellationToken.None);

        // Assert
        Assert.False(_cache.TryGetValue(CacheKeys.Rules.ActiveRules, out _));
        _evaluatorMock.Verify(e => e.ClearCache(), Times.Once);
    }

    [Fact]
    public async Task Update_InvalidatesCache()
    {
        // Arrange
        var rule = ActiveRule("Rule-U");
        _db.RuleConfigurations.Add(rule);
        await _db.SaveChangesAsync();
        await _sut.GetActiveRulesAsync(CancellationToken.None);

        Assert.True(_cache.TryGetValue(CacheKeys.Rules.ActiveRules, out _));

        // Act
        _sut.Update(rule);

        // Assert
        Assert.False(_cache.TryGetValue(CacheKeys.Rules.ActiveRules, out _));
        _evaluatorMock.Verify(e => e.ClearCache(), Times.Once);
    }

    [Fact]
    public async Task GetActiveRulesAsync_AfterCacheInvalidation_QueriesDbAgain()
    {
        // Arrange
        var rule = ActiveRule("Rule-Z");
        _db.RuleConfigurations.Add(rule);
        await _db.SaveChangesAsync();
        await _sut.GetActiveRulesAsync(CancellationToken.None);

        // Invalidate
        _sut.ClearCache();

        // Add
        var secondRule = ActiveRule("Rule-ZZ");
        _db.RuleConfigurations.Add(secondRule);
        await _db.SaveChangesAsync();

        // Act
        var result = await _sut.GetActiveRulesAsync(CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
    }
}
