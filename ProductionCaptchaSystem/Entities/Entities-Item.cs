using System;

namespace ProductionCaptchaSystem.Entities;

public partial class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Article { get; set; }
    public string Unit { get; set; } = null!;
    public decimal Price { get; set; }
    public int TypeId { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual ItemType Type { get; set; } = null!;
}
