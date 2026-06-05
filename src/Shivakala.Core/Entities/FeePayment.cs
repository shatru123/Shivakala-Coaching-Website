namespace Shivakala.Core.Entities;

public sealed class FeePayment : BaseEntity
{
    public int       StudentId       { get; set; }
    public required string FeeType   { get; set; }   // Admission | Monthly | Exam | Annual
    public decimal   Amount          { get; set; }
    public decimal   Discount        { get; set; } = 0;
    public decimal   Fine            { get; set; } = 0;
    public decimal   PaidAmount      { get; set; }
    public string    PaymentMode     { get; set; } = "Cash"; // Cash | UPI | Cheque | Online
    public string?   TransactionRef  { get; set; }
    public string    Month           { get; set; } = DateTime.UtcNow.ToString("yyyy-MM");
    public string    Status          { get; set; } = "Paid"; // Paid | Partial | Pending | Waived
    public string?   ReceiptNumber   { get; set; }
    public string?   Remarks         { get; set; }
    public DateTime  PaidDate        { get; set; } = DateTime.UtcNow;
    public DateTime  CreatedDate     { get; set; } = DateTime.UtcNow;
    public int?      CollectedByUserId { get; set; }

    public Student?  Student { get; set; }
}
