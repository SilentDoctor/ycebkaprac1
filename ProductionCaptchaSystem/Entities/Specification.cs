namespace ProductionCaptchaSystem.Entities;

public partial class Specification
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual Item Product { get; set; } = null!;
}