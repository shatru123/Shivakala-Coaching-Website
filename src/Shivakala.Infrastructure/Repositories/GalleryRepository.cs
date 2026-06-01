using Microsoft.EntityFrameworkCore;
using Shivakala.Core.Entities;
using Shivakala.Infrastructure.Data;

namespace Shivakala.Infrastructure.Repositories;

public interface IGalleryRepository
{
    Task<List<GalleryItem>> GetActiveAsync(string? category=null,CancellationToken ct=default);
    Task<List<GalleryItem>> GetAllAdminAsync(CancellationToken ct=default);
    Task<List<string>> GetCategoriesAsync(CancellationToken ct=default);
    Task AddAsync(GalleryItem item,CancellationToken ct=default);
    Task UpdateAsync(GalleryItem item,CancellationToken ct=default);
    Task DeleteAsync(int id,CancellationToken ct=default);
}

public sealed class GalleryRepository(ShivakalaDbContext db) : IGalleryRepository
{
    public Task<List<GalleryItem>> GetActiveAsync(string? category=null,CancellationToken ct=default)
    {
        var q=db.GalleryItems.Where(g=>g.IsActive);
        if(!string.IsNullOrWhiteSpace(category)) q=q.Where(g=>g.Category==category);
        return q.OrderBy(g=>g.DisplayOrder).ThenByDescending(g=>g.CreatedDate).ToListAsync(ct);
    }
    public Task<List<GalleryItem>> GetAllAdminAsync(CancellationToken ct=default) =>
        db.GalleryItems.OrderByDescending(g=>g.CreatedDate).ToListAsync(ct);
    public Task<List<string>> GetCategoriesAsync(CancellationToken ct=default) =>
        db.GalleryItems.Where(g=>g.IsActive).Select(g=>g.Category).Distinct().ToListAsync(ct);
    public async Task AddAsync(GalleryItem item,CancellationToken ct=default){db.GalleryItems.Add(item);await db.SaveChangesAsync(ct);}
    public async Task UpdateAsync(GalleryItem item,CancellationToken ct=default){db.GalleryItems.Update(item);await db.SaveChangesAsync(ct);}
    public async Task DeleteAsync(int id,CancellationToken ct=default){
        var g=await db.GalleryItems.FindAsync([id],ct);
        if(g!=null){db.GalleryItems.Remove(g);await db.SaveChangesAsync(ct);}
    }
}
