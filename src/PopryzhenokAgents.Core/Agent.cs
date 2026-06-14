namespace Exam.Core;

public sealed class Agent
{
    public int Id { get; set; }
    public int AgentTypeId { get; set; }
    public string Title { get; set; } = "";
    public string Type { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Logo { get; set; } = "picture.png";
    public string Address { get; set; } = "";
    public string Director { get; set; } = "";
    public string Inn { get; set; } = "";
    public string Kpp { get; set; } = "";
    public int Priority { get; set; }
    public decimal SalesAmount365 { get; set; }
    public int SalesCount365 { get; set; }
    public bool HasProductSale { get; set; }
    public int Discount => ListTools.DiscountBySales(SalesAmount365);
    public string Highlight => Discount >= 25 ? "#ccffcc" : "White";
}
