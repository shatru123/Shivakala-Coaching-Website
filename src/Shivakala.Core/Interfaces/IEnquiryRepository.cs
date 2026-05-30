using Shivakala.Core.Entities;

namespace Shivakala.Core.Interfaces;

public interface IEnquiryRepository : IRepository<Enquiry>
{
    Task<IReadOnlyList<Enquiry>> ListRecentAsync(CancellationToken cancellationToken = default);
}
