using Microsoft.EntityFrameworkCore;
using Shivakala.Core.Entities;
using Shivakala.Core.Interfaces;
using Shivakala.Infrastructure.Data;

namespace Shivakala.Infrastructure.Repositories;

public sealed class FeeRepository(ShivakalaDbContext db) : IFeeRepository
{
    public Task<IReadOnlyList<FeePayment>> GetByStudentAsync(int studentId, CancellationToken ct)
        => db.FeePayments.Where(f => f.StudentId == studentId)
             .OrderByDescending(f => f.PaidDate).ToListAsync(ct)
             .ContinueWith(t => (IReadOnlyList<FeePayment>)t.Result, ct);

    public Task<IReadOnlyList<FeePayment>> GetAllAsync(string? month, string? status, CancellationToken ct)
    {
        var q = db.FeePayments.Include(f => f.Student).AsQueryable();
        if (!string.IsNullOrWhiteSpace(month)) q = q.Where(f => f.Month == month);
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(f => f.Status == status);
        return q.OrderByDescending(f => f.PaidDate).ToListAsync(ct)
                .ContinueWith(t => (IReadOnlyList<FeePayment>)t.Result, ct);
    }

    public Task<FeePayment?> GetByIdAsync(int id, CancellationToken ct)
        => db.FeePayments.Include(f => f.Student).FirstOrDefaultAsync(f => f.Id == id, ct);

    public async Task<FeePayment> AddAsync(FeePayment payment, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(payment.ReceiptNumber))
            payment.ReceiptNumber = await GenerateReceiptNumberAsync(ct);
        db.FeePayments.Add(payment);
        await db.SaveChangesAsync(ct);
        return payment;
    }

    public async Task UpdateAsync(FeePayment payment, CancellationToken ct)
    { db.FeePayments.Update(payment); await db.SaveChangesAsync(ct); }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var p = await db.FeePayments.FindAsync([id], ct);
        if (p is not null) { db.FeePayments.Remove(p); await db.SaveChangesAsync(ct); }
    }

    public async Task<decimal> GetTotalCollectedAsync(string month, CancellationToken ct)
        => (decimal)(await db.FeePayments.Where(f => f.Month == month && f.Status == "Paid")
             .SumAsync(f => (double?)f.PaidAmount, ct) ?? 0);

    public async Task<decimal> GetTotalPendingAsync(CancellationToken ct)
        => (decimal)(await db.FeePayments.Where(f => f.Status == "Pending")
             .SumAsync(f => (double?)f.Amount - (double?)f.PaidAmount, ct) ?? 0);

    public async Task<string> GenerateReceiptNumberAsync(CancellationToken ct)
    {
        var count = await db.FeePayments.CountAsync(ct);
        return $"RCP{DateTime.UtcNow:yyyyMM}{(count + 1):D4}";
    }

    public Task<IReadOnlyList<FeeStructure>> GetFeeStructuresAsync(CancellationToken ct)
        => db.FeeStructures.OrderBy(f => f.Standard).ToListAsync(ct)
             .ContinueWith(t => (IReadOnlyList<FeeStructure>)t.Result, ct);

    public async Task<FeeStructure> AddFeeStructureAsync(FeeStructure s, CancellationToken ct)
    { db.FeeStructures.Add(s); await db.SaveChangesAsync(ct); return s; }

    public async Task UpdateFeeStructureAsync(FeeStructure s, CancellationToken ct)
    { db.FeeStructures.Update(s); await db.SaveChangesAsync(ct); }

    public async Task DeleteFeeStructureAsync(int id, CancellationToken ct)
    {
        var s = await db.FeeStructures.FindAsync([id], ct);
        if (s is not null) { db.FeeStructures.Remove(s); await db.SaveChangesAsync(ct); }
    }
}
