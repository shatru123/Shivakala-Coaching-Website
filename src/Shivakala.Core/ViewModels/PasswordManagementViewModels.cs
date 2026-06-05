using System.ComponentModel.DataAnnotations;

namespace Shivakala.Core.ViewModels;

public sealed class ChangePasswordViewModel
{
    [Required, DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, MinLength(6), DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed class PortalAccountAdminViewModel
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Mobile { get; set; }
    public string LinkedTo { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public sealed class AdminResetPasswordViewModel
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    [Required, MinLength(6), DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; } = string.Empty;
}
