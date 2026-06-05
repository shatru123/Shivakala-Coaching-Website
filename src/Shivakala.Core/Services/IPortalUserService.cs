using Shivakala.Core.Entities;

namespace Shivakala.Core.Services;

public interface IPortalUserService
{
    Task<AppUser?> ValidateCredentialsAsync(string username, string password, string role, CancellationToken ct = default);
    Task<AppUser?> FindByIdAsync(int userId, CancellationToken ct = default);
    Task<AppUser?> FindTeacherUserAsync(string login, CancellationToken ct = default);
    Task<AppUser> EnsureTeacherAccountAsync(int teacherId, string? username = null, string? password = null, CancellationToken ct = default);
    Task<AppUser?> EnsureParentAccountForStudentAsync(int studentId, string? password = null, CancellationToken ct = default);
    Task<AppUser> EnsureAdminAccountAsync(string username, string password, CancellationToken ct = default);
    Task<(bool Success, string ErrorMessage)> ChangePasswordAsync(int userId, string currentPassword, string newPassword, CancellationToken ct = default);
    Task<(bool Success, string ErrorMessage)> SetPasswordAsync(int userId, string newPassword, CancellationToken ct = default);
    Task SyncMissingPortalAccountsAsync(CancellationToken ct = default);
}
