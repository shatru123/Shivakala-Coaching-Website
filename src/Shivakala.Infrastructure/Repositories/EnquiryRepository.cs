using Shivakala.Core.Entities;
using Shivakala.Core.Interfaces;
using Shivakala.Infrastructure.Data;

namespace Shivakala.Infrastructure.Repositories;

public sealed class EnquiryRepository(ShivakalaDbContext dbContext) : Repository<Enquiry>(dbContext), IEnquiryRepository
{
}
