using Microsoft.Extensions.Logging;
using Shivakala.Core.Entities;
using Shivakala.Core.Interfaces;
using Shivakala.Core.Services;
using Shivakala.Core.ViewModels;

namespace Shivakala.Infrastructure.Services;

public sealed class EnquiryService(
    IEnquiryRepository enquiryRepository,
    ILogger<EnquiryService> logger) : IEnquiryService
{
    public async Task SubmitEnquiryAsync(EnquiryFormViewModel model, CancellationToken cancellationToken = default)
    {
        var enquiry = new Enquiry
        {
            Name = model.Name.Trim(),
            Mobile = model.Mobile.Trim(),
            Message = model.Message.Trim(),
            CreatedDate = DateTime.UtcNow
        };

        await enquiryRepository.AddAsync(enquiry, cancellationToken);
        logger.LogInformation("Enquiry saved for {Name}", enquiry.Name);
    }
}
