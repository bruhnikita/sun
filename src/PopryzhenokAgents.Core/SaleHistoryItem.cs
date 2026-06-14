namespace Exam.Core;

public sealed class SaleHistoryItem
{
    public DateTime SaleDate { get; set; }
    public string ProductTitle { get; set; } = "";
    public int ProductCount { get; set; }
    public decimal Amount { get; set; }
}
