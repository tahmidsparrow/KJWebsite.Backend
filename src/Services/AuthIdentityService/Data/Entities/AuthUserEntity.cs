namespace AuthIdentityService.Data.Entities;

public sealed class AuthUserEntity
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "editor";
    public string Status { get; set; } = "active";
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
    public string? PermanentAddress { get; set; }
    public string? MailingAddress { get; set; }
    public bool IsMailingAddressSameAsPermanentAddress { get; set; }
    public string? BloodGroup { get; set; }
    public string? AreasOfExpertise { get; set; }
    public string? HighestDegree { get; set; }
    public string? DisabilitiesIfAny { get; set; }
    public string? Nationality { get; set; }
    public string? PersonalWebPage { get; set; }
    public string? SocialMediaLink { get; set; }
}
