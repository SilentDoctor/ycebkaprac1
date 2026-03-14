namespace ProductionCaptchaSystem.Entities;

public partial class CustomerOrder
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = null!;
    public int CounterpartyId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public string Status { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public string? Comment { get; set; }

    public virtual Counterparty Counterparty { get; set; } = null!;
}