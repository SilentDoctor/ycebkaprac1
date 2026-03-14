namespace ProductionCaptchaSystem.Models;

public class FullNameModel
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Patronymic { get; set; }

    public override string ToString()
    {
        return $"{LastName} {FirstName} {Patronymic}".Trim();
    }
}