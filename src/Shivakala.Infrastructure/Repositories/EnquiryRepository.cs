using Microsoft.EntityFrameworkCore;
using Shivakala.Core.Entities;
using Shivakala.Core.Interfaces;
using Shivakala.Infrastructure.Data;

namespace Shivakala.Infrastructure.Repositories;

public sealed class EnquiryRepository(ShivakalaDbContext dbContext) : Repository<Enquiry>(dbContext), IEnquiryRepository
{
    public async Task<IReadOnlyList<Enquiry>> ListRecentAsync(CancellationToken cancellationToken = default)
        => await DbContext.Enquiries
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync(cancellationToken);
}
