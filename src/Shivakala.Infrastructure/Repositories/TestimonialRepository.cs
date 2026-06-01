using Microsoft.EntityFrameworkCore;
using Shivakala.Core.Entities;
using Shivakala.Infrastructure.Data;

namespace Shivakala.Infrastructure.Repositories;

public interface ITestimonialRepository
{
    Task<List<Testimonial>> GetApprovedAsync(bool featuredOnly=false,CancellationToken ct=default);
    Task<List<Testimonial>> GetAllAdminAsync(CancellationToken ct=default);
    Task<Testimonial?> GetByIdAsync(int id,CancellationToken ct=default);
    Task AddAsync(Testimonial t,CancellationToken ct=default);
    Task UpdateAsync(Testimonial t,CancellationToken ct=default);
    Task DeleteAsync(int id,CancellationToken ct=default);
}

public sealed class TestimonialRepository(ShivakalaDbContext db) : ITestimonialRepository
{
    public Task<List<Testimonial>> GetApprovedAsync(bool featuredOnly=false,CancellationToken ct=default)
    {
        var q=db.Testimonials.Where(t=>t.IsApproved);
        if(featuredOnly) q=q.Where(t=>t.IsFeatured);
        return q.OrderByDescending(t=>t.Rating).ThenByDescending(t=>t.CreatedDate).ToListAsync(ct);
    }
    public Task<List<Testimonial>> GetAllAdminAsync(CancellationToken ct=default) =>
        db.Testimonials.OrderByDescending(t=>t.CreatedDate).ToListAsync(ct);
    public Task<Testimonial?> GetByIdAsync(int id,CancellationToken ct=default) =>
        db.Testimonials.FirstOrDefaultAsync(t=>t.Id==id,ct);
    public async Task AddAsync(Testimonial t,CancellationToken ct=default){db.Testimonials.Add(t);await db.SaveChangesAsync(ct);}
    public async Task UpdateAsync(Testimonial t,CancellationToken ct=default){db.Testimonials.Update(t);await db.SaveChangesAsync(ct);}
    public async Task DeleteAsync(int id,CancellationToken ct=default){
        var t=await db.Testimonials.FindAsync([id],ct);
        if(t!=null){db.Testimonials.Remove(t);await db.SaveChangesAsync(ct);}
    }
}
