using Microsoft.EntityFrameworkCore;
using Shivakala.Core.Entities;
using Shivakala.Infrastructure.Data;

namespace Shivakala.Infrastructure.Repositories;

public interface IStudyMaterialRepository
{
    Task<List<StudyMaterial>> GetActiveAsync(string? standard=null,string? subject=null,string? type=null,CancellationToken ct=default);
    Task<List<StudyMaterial>> GetAllAdminAsync(CancellationToken ct=default);
    Task<StudyMaterial?> GetByIdAsync(int id,CancellationToken ct=default);
    Task AddAsync(StudyMaterial m,CancellationToken ct=default);
    Task UpdateAsync(StudyMaterial m,CancellationToken ct=default);
    Task DeleteAsync(int id,CancellationToken ct=default);
}

public sealed class StudyMaterialRepository(ShivakalaDbContext db) : IStudyMaterialRepository
{
    public Task<List<StudyMaterial>> GetActiveAsync(string? standard=null,string? subject=null,string? type=null,CancellationToken ct=default)
    {
        var q = db.StudyMaterials.Where(m => m.IsActive);
        if (!string.IsNullOrWhiteSpace(standard)) q = q.Where(m => m.Standard==standard);
        if (!string.IsNullOrWhiteSpace(subject)) q = q.Where(m => m.Subject==subject);
        if (!string.IsNullOrWhiteSpace(type)) q = q.Where(m => m.MaterialType==type);
        return q.OrderByDescending(m => m.UploadedDate).ToListAsync(ct);
    }
    public Task<List<StudyMaterial>> GetAllAdminAsync(CancellationToken ct=default) =>
        db.StudyMaterials.OrderByDescending(m => m.UploadedDate).ToListAsync(ct);
    public Task<StudyMaterial?> GetByIdAsync(int id,CancellationToken ct=default) =>
        db.StudyMaterials.FirstOrDefaultAsync(m => m.Id==id,ct);
    public async Task AddAsync(StudyMaterial m,CancellationToken ct=default){db.StudyMaterials.Add(m);await db.SaveChangesAsync(ct);}
    public async Task UpdateAsync(StudyMaterial m,CancellationToken ct=default){db.StudyMaterials.Update(m);await db.SaveChangesAsync(ct);}
    public async Task DeleteAsync(int id,CancellationToken ct=default){
        var m=await db.StudyMaterials.FindAsync([id],ct);
        if(m!=null){db.StudyMaterials.Remove(m);await db.SaveChangesAsync(ct);}
    }
}
