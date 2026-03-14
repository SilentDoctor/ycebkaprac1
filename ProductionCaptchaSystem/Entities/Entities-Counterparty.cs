using System;

namespace ProductionCaptchaSystem.Entities;

public partial class Counterparty
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Inn { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public DateTime CreatedAt { get; set; }
}
