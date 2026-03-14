namespace ProductionCaptchaSystem.Entities;

public partial class ProductionOrderProduct
{
    public int Id { get; set; }
    public int ProductionOrderId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal ProducedQuantity { get; set; }

    public virtual ProductionOrder ProductionOrder { get; set; } = null!;
    public virtual Item Product { get; set; } = null!;
}