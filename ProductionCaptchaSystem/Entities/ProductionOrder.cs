namespace ProductionCaptchaSystem.Entities;

public partial class ProductionOrder
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = null!;
    public int? CustomerOrderId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? PlannedEndDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public string Status { get; set; } = null!;
    public string? Comment { get; set; }

    public virtual CustomerOrder? CustomerOrder { get; set; }
}