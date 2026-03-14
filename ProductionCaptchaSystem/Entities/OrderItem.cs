namespace ProductionCaptchaSystem.Entities;

public partial class OrderItem
{
    public int Id { get; set; }
    public int CustomerOrderId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Amount { get; set; }

    public virtual CustomerOrder CustomerOrder { get; set; } = null!;
    public virtual Item Product { get; set; } = null!;
}