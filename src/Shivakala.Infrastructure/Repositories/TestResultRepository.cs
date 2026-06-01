using Microsoft.EntityFrameworkCore;
using Shivakala.Core.Entities;
using Shivakala.Infrastructure.Data;

namespace Shivakala.Infrastructure.Repositories;

public interface ITestResultRepository
{
    Task<List<TestResult>> GetByTestAsync(string testTitle, string? standard = null, CancellationToken ct = default);
    Task<List<string>> GetTestTitlesAsync(CancellationToken ct = default);
    Task<List<TestResult>> GetAllAdminAsync(CancellationToken ct = default);
    Task AddAsync(TestResult result, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<TestResult> results, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task DeleteByTestAsync(string testTitle, CancellationToken ct = default);
}

public sealed class TestResultRepository(ShivakalaDbContext db) : ITestResultRepository
{
    public Task<List<TestResult>> GetByTestAsync(string testTitle, string? standard = null, CancellationToken ct = default)
    {
        var q = db.TestResults.Where(t => t.TestTitle == testTitle);
        if (!string.IsNullOrWhiteSpace(standard)) q = q.Where(t => t.Standard == standard);
        return q.OrderBy(t => t.Rank).ToListAsync(ct);
    }
    public Task<List<string>> GetTestTitlesAsync(CancellationToken ct = default) =>
        db.TestResults.Select(t => t.TestTitle).Distinct().OrderByDescending(t => t).ToListAsync(ct);
    public Task<List<TestResult>> GetAllAdminAsync(CancellationToken ct = default) =>
        db.TestResults.OrderByDescending(t => t.TestDate).ToListAsync(ct);
    public async Task AddAsync(TestResult r, CancellationToken ct = default) {
        db.TestResults.Add(r); await db.SaveChangesAsync(ct);
    }
    public async Task AddRangeAsync(IEnumerable<TestResult> results, CancellationToken ct = default) {
        db.TestResults.AddRange(results); await db.SaveChangesAsync(ct);
    }
    public async Task DeleteAsync(int id, CancellationToken ct = default) {
        var r = await db.TestResults.FindAsync([id], ct);
        if (r != null) { db.TestResults.Remove(r); await db.SaveChangesAsync(ct); }
    }
    public async Task DeleteByTestAsync(string testTitle, CancellationToken ct = default) {
        var items = db.TestResults.Where(t => t.TestTitle == testTitle);
        db.TestResults.RemoveRange(items); await db.SaveChangesAsync(ct);
    }
}
