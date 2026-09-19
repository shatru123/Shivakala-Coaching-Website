namespace Shivakala.Core.Entities;

/// <summary>
/// Dedicated entity for MPSC candidates, completely decoupled from school students.
/// </summary>
public sealed class MpscStudent : BaseEntity
{
    // Unique human-readable registration number (e.g. SK-MPSC-2026-00001)
    public required string RegistrationNumber { get; set; }

    // Personal details
    public required string FullName         { get; set; }
    public string?   FullNameMarathi        { get; set; }
    public required string Email            { get; set; }
    public required string Mobile           { get; set; }
    public string?   AlternateMobile        { get; set; }
    public required string DateOfBirth      { get; set; } // stored as "yyyy-MM-dd"
    public required string Gender           { get; set; } // Male | Female | Other
    public string?   PhotoUrl               { get; set; }

    // Address
    public required string Address          { get; set; }
    public string?   City                   { get; set; }
    public required string District         { get; set; }
    public string    State                  { get; set; } = "Maharashtra";
    public string?   Pincode                { get; set; }

    // Educational details
    public required string HighestQualification { get; set; } // 10th | 12th | Diploma | Graduate | PostGraduate | Other
    public string?   DegreeOrCourse         { get; set; }
    public string?   University             { get; set; }
    public int?      YearOfPassing          { get; set; }

    // MPSC examination preferences
    public string    PreferredExam          { get; set; } = "Rajyaseva"; // Rajyaseva | Combine | Group B | Group C | PSI | STI | ASO | Other
    public string?   OtherExamInterest      { get; set; }
    public string    PreparationLevel       { get; set; } = "Beginner"; // Beginner | Intermediate | Advanced | Already Appeared
    public int?      TargetAttemptYear      { get; set; }
    public bool      PreviousAttempts       { get; set; } = false;
    public int?      NumberOfPreviousAttempts { get; set; } = 0;

    // Category & reservation details (informational)
    public string    Category               { get; set; } = "Open"; // Open | OBC | SC | ST | VJ/NT | EWS | Other
    public bool      CasteCertificateAvailable { get; set; } = false;
    public bool      NonCreamyLayerCertificateAvailable { get; set; } = false;
    public bool      EwsCertificateAvailable { get; set; } = false;

    // Terms & consent
    public bool      ConsentAccepted        { get; set; } = true;
    public DateTime? ConsentAcceptedAt      { get; set; } = DateTime.UtcNow;
    public string?   ConsentIpAddress       { get; set; }

    // Account status & timestamps
    public bool      IsActive               { get; set; } = true;
    public DateTime  RegistrationDate       { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdatedDate        { get; set; }

    // Navigation collections
    public ICollection<MpscExamRegistration> Registrations { get; set; } = [];
    public ICollection<MpscExamResult> Results             { get; set; } = [];
    public ICollection<OnlineExamAttempt> OnlineAttempts   { get; set; } = [];
}
