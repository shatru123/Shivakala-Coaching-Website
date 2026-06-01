using Microsoft.Extensions.Logging;
using Shivakala.Core.Entities;
using Shivakala.Core.Interfaces;
using Shivakala.Core.Services;
using Shivakala.Core.ViewModels;

namespace Shivakala.Infrastructure.Services;

public sealed class RegistrationService(
    IStudentRepository studentRepository,
    ILogger<RegistrationService> logger) : IRegistrationService
{
    public async Task RegisterStudentAsync(RegistrationFormViewModel model, CancellationToken ct = default)
        => await RegisterAsync(model, ct);

    public async Task RegisterAsync(RegistrationFormViewModel model, CancellationToken ct = default)
    {
        var student = new Student
        {
            FullName    = model.FullName.Trim(),
            ParentName  = string.IsNullOrWhiteSpace(model.ParentName) ? null : model.ParentName.Trim(),
            Mobile      = model.Mobile.Trim(),
            Email       = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim(),
            Standard    = model.Standard.Trim(),
            Subject     = model.Subject.Trim(),
            Address     = model.Address.Trim(),
            Board       = string.IsNullOrWhiteSpace(model.Board) ? null : model.Board.Trim(),
            Medium      = string.IsNullOrWhiteSpace(model.Medium) ? null : model.Medium.Trim(),
            Status      = "Pending",
            CreatedDate = DateTime.UtcNow
        };
        await studentRepository.AddAsync(student, ct);
        logger.LogInformation("Student registration saved: {Name} for Std {Standard}", student.FullName, student.Standard);
    }

    public Task<RegistrationFormViewModel> GetFormViewModelAsync(CancellationToken ct = default)
        => Task.FromResult(new RegistrationFormViewModel
        {
            Seo = new SeoViewModel
            {
                Title = "Student Registration | Shivakala Coaching Classes",
                Description = "Register your child for Shivakala Coaching Classes admission 2026-27. KG1 to 10th Standard."
            }
        });
}
