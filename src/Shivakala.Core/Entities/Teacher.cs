namespace Shivakala.Core.Entities;

public sealed class Teacher : BaseEntity
{
    public required string FullName       { get; set; }
    public required string Mobile         { get; set; }
    public string?   Email                { get; set; }
    public string?   Qualification        { get; set; }
    public string?   Specialisation       { get; set; }   // comma-separated subjects
    public string?   PhotoUrl             { get; set; }
    public string?   Address              { get; set; }
    public string?   EmployeeCode         { get; set; }
    public decimal?  MonthlySalary        { get; set; }
    public DateTime  JoiningDate          { get; set; } = DateTime.UtcNow;
    public bool      IsActive             { get; set; } = true;
    public bool      ShowOnAboutPage      { get; set; } = true;
    public string?   PublicDesignation    { get; set; }
    public string?   PublicDesignationMarathi { get; set; }
    public string?   PublicExperience     { get; set; }
    public string?   PublicExperienceMarathi { get; set; }
    public string?   AdminNotes           { get; set; }
    public DateTime  CreatedDate          { get; set; } = DateTime.UtcNow;
}
