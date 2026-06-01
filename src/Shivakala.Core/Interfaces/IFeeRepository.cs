using Shivakala.Core.Entities;

namespace Shivakala.Core.Interfaces;

public interface IFeeRepository
{
    Task<IReadOnlyList<FeePayment>> GetByStudentAsync(int studentId, CancellationToken ct = default);
    Task<IReadOnlyList<FeePayment>> GetAllAsync(string? month = null, string? status = null, CancellationToken ct = default);
    Task<FeePayment?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<FeePayment> AddAsync(FeePayment payment, CancellationToken ct = default);
    Task UpdateAsync(FeePayment payment, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<decimal> GetTotalCollectedAsync(string month, CancellationToken ct = default);
    Task<decimal> GetTotalPendingAsync(CancellationToken ct = default);
    Task<string> GenerateReceiptNumberAsync(CancellationToken ct = default);
    Task<IReadOnlyList<FeeStructure>> GetFeeStructuresAsync(CancellationToken ct = default);
    Task<FeeStructure> AddFeeStructureAsync(FeeStructure structure, CancellationToken ct = default);
    Task UpdateFeeStructureAsync(FeeStructure structure, CancellationToken ct = default);
    Task DeleteFeeStructureAsync(int id, CancellationToken ct = default);
}
