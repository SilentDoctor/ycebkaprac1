namespace ProductionCaptchaSystem.Entities;

public partial class SpecificationLine
{
    public int Id { get; set; }
    public int SpecificationId { get; set; }
    public int MaterialId { get; set; }
    public decimal Quantity { get; set; }

    public virtual Specification Specification { get; set; } = null!;
    public virtual Item Material { get; set; } = null!;
}