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
    public async Task RegisterStudentAsync(RegistrationFormViewModel model, CancellationToken cancellationToken = default)
    {
        var student = new Student
        {
            FullName = model.FullName.Trim(),
            Mobile = model.Mobile.Trim(),
            Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim(),
            Standard = model.Standard.Trim(),
            Subject = model.Subject.Trim(),
            Address = model.Address.Trim(),
            CreatedDate = DateTime.UtcNow
        };

        await studentRepository.AddAsync(student, cancellationToken);
        logger.LogInformation("Student registration saved for {StudentName}", student.FullName);
    }
}
