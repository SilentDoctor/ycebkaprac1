namespace ProductionCaptchaSystem.Entities;

public partial class ProductionOrderMaterial
{
    public int Id { get; set; }
    public int ProductionOrderId { get; set; }
    public int MaterialId { get; set; }
    public decimal RequiredQuantity { get; set; }
    public decimal UsedQuantity { get; set; }

    public virtual ProductionOrder ProductionOrder { get; set; } = null!;
    public virtual Item Material { get; set; } = null!;
}