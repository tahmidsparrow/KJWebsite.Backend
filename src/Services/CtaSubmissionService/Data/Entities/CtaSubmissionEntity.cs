namespace CtaSubmissionService.Data.Entities;

public sealed class CtaSubmissionEntity
{
    public string Id { get; set; } = string.Empty;
    public string FormName { get; set; } = string.Empty;
    public string CtaType { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string SourcePath { get; set; } = string.Empty;
    public string ValuesJson { get; set; } = "{}";
    public DateTimeOffset SubmittedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Gender { get; set; }
    public string? ReasonForJoining { get; set; }
    public string? PresentOrganization { get; set; }
    public string? VolunteeingExperience { get; set; }
    public string? DateOfBirth { get; set; }
    public string? CityOfResidence { get; set; }
    public string? CountryOfResidence { get; set; }
}
