var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var submissions = new List<CtaSubmissionRecord>();
var allowedForms = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "registration", "contact", "footer-contact", "project-involved" };
var allowedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "join", "donate", "partnership", "get-involved", "contact" };
var allowedLangs = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "bn", "en" };

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "CtaSubmissionService" }));

app.MapPost("/api/v1/cta/submissions", (CtaSubmissionRequest payload) =>
{
    if (!allowedForms.Contains(payload.FormName) || !allowedTypes.Contains(payload.CtaType) || !allowedLangs.Contains(payload.Language))
    {
        return Results.BadRequest(ApiError("VALIDATION_ERROR", "Invalid formName, ctaType, or language."));
    }

    var serializedValues = System.Text.Json.JsonSerializer.Serialize(payload.Values);
    if (serializedValues.Length > 32_000)
    {
        return Results.Json(ApiError("PAYLOAD_TOO_LARGE", "values payload exceeds 32KB."), statusCode: StatusCodes.Status413PayloadTooLarge);
    }

    var id = $"subm_{Guid.NewGuid():N}";
    var createdAt = DateTimeOffset.UtcNow;
    submissions.Add(new CtaSubmissionRecord(id, payload.FormName, payload.CtaType, payload.Language, payload.SourcePath, payload.Values, payload.SubmittedAt, createdAt));

    return Results.Created($"/api/v1/cta/submissions/{id}", new { id, status = "accepted", created_at = createdAt });
});

app.Run();

static object ApiError(string code, string message) => new { error = new { code, message } };

record CtaSubmissionRequest(string FormName, string CtaType, string Language, string SourcePath, Dictionary<string, object?> Values, DateTimeOffset SubmittedAt);
record CtaSubmissionRecord(string Id, string FormName, string CtaType, string Language, string SourcePath, Dictionary<string, object?> Values, DateTimeOffset SubmittedAt, DateTimeOffset CreatedAt);
