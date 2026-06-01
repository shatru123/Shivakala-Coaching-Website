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
    public async Task SubmitEnquiryAsync(EnquiryFormViewModel model, CancellationToken ct = default)
    {
        var enquiry = new Enquiry
        {
            Name           = model.Name.Trim(),
            Mobile         = model.Mobile.Trim(),
            Email          = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim(),
            Message        = model.Message.Trim(),
            ClassInterested= string.IsNullOrWhiteSpace(model.ClassInterested) ? null : model.ClassInterested.Trim(),
            IsRead         = false,
            CreatedDate    = DateTime.UtcNow
        };
        await enquiryRepository.AddAsync(enquiry, ct);
        logger.LogInformation("Enquiry submitted: {Name}", enquiry.Name);
    }

    public Task<EnquiryFormViewModel> GetFormViewModelAsync(CancellationToken ct = default)
        => Task.FromResult(new EnquiryFormViewModel
        {
            Seo = new SeoViewModel
            {
                Title = "Enquiry | Shivakala Coaching Classes",
                Description = "Send an enquiry to Shivakala Coaching Classes, Chikhali, Maharashtra."
            }
        });
}
