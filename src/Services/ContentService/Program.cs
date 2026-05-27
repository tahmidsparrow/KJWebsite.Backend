using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


var projects = new List<ProjectItem>
{
    new("pathshala", "pathshala", "en", "Pathshala", "Pathshala", "Training project", "Full details", "/img/causes/2.jpg", "Education", "2016", "active", "flaticon-circle", DateTimeOffset.UtcNow),
    new("pathshala", "pathshala", "bn", "পাঠশালা", "পাঠশালা", "প্রশিক্ষণ প্রকল্প", "বিস্তারিত", "/img/causes/2.jpg", "শিক্ষা", "২০১৬", "active", "flaticon-circle", DateTimeOffset.UtcNow)
};

var news = new List<NewsItem>
{
    new("bauniabadh-mou", "bauniabadh-mou", "en", "MoU signed", "Summary", ["Paragraph 1"], "/img/blog/1.jpg", "February 25, 2017", DateTimeOffset.Parse("2017-02-25", CultureInfo.InvariantCulture), "published"),
    new("bauniabadh-mou", "bauniabadh-mou", "bn", "সমঝোতা চুক্তি", "সারসংক্ষেপ", ["অনুচ্ছেদ ১"], "/img/blog/1.jpg", "ফেব্রুয়ারি ২৫, ২০১৭", DateTimeOffset.Parse("2017-02-25", CultureInfo.InvariantCulture), "published")
};

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "ContentService" }));

app.MapGet("/api/v1/content/projects", (string? lang, string? status) =>
{
    var normalizedLang = NormalizeLang(lang);
    var normalizedStatus = string.IsNullOrWhiteSpace(status) ? "active" : status.Trim().ToLowerInvariant();
    if (normalizedStatus is not ("active" or "all"))
    {
        return Results.BadRequest(ApiError("VALIDATION_ERROR", "status must be active or all."));
    }

    var filtered = projects.Where(p => p.Lang == normalizedLang && (normalizedStatus == "all" || p.Status == "active")).ToArray();
    return Results.Ok(new { items = filtered });
});

app.MapGet("/api/v1/content/projects/{id}", (string id, string? lang) =>
{
    var normalizedLang = NormalizeLang(lang);
    var item = projects.FirstOrDefault(p => p.Id == id && p.Lang == normalizedLang);
    return item is null
        ? Results.NotFound(ApiError("NOT_FOUND", "Project not found."))
        : Results.Ok(item);
});

app.MapGet("/api/v1/content/news", (string? lang, int? limit, int? offset) =>
{
    var normalizedLang = NormalizeLang(lang);
    var safeLimit = limit.GetValueOrDefault(20);
    var safeOffset = offset.GetValueOrDefault(0);

    if (safeLimit is < 1 or > 100 || safeOffset < 0)
    {
        return Results.BadRequest(ApiError("VALIDATION_ERROR", "Invalid paging values."));
    }

    var langRows = news.Where(n => n.Lang == normalizedLang).ToList();
    var paged = langRows.Skip(safeOffset).Take(safeLimit).ToArray();

    return Results.Ok(new { items = paged, total = langRows.Count, limit = safeLimit, offset = safeOffset });
});

app.MapGet("/api/v1/content/news/{id}", (string id, string? lang) =>
{
    var normalizedLang = NormalizeLang(lang);
    var item = news.FirstOrDefault(n => n.Id == id && n.Lang == normalizedLang);
    return item is null
        ? Results.NotFound(ApiError("NOT_FOUND", "News item not found."))
        : Results.Ok(item);
});

app.MapPost("/api/v1/admin/content/projects", (HttpRequest request, ProjectUpsertRequest payload) =>
{
    var authResult = EnsureAdmin(request);
    if (authResult is not null) return authResult;

    if (payload.Translations.Count == 0)
    {
        return Results.BadRequest(ApiError("VALIDATION_ERROR", "At least one translation is required."));
    }

    var created = payload.Translations.Select(t => new ProjectItem(
        payload.Slug,
        payload.Slug,
        t.Lang,
        t.Title,
        t.ShortTitle ?? t.Title,
        t.ShortDescription ?? string.Empty,
        t.FullDescription ?? string.Empty,
        payload.ImageUrl ?? string.Empty,
        payload.Category ?? string.Empty,
        payload.Year ?? string.Empty,
        payload.Status,
        payload.Icon ?? string.Empty,
        DateTimeOffset.UtcNow)).ToList();

    projects.RemoveAll(p => p.Id == payload.Slug);
    projects.AddRange(created);

    return Results.Created($"/api/v1/content/projects/{payload.Slug}", created.First());
});

app.MapPut("/api/v1/admin/content/projects/{id}", (HttpRequest request, string id, ProjectUpsertRequest payload) =>
{
    var authResult = EnsureAdmin(request);
    if (authResult is not null) return authResult;

    if (!projects.Any(p => p.Id == id))
    {
        return Results.NotFound(ApiError("NOT_FOUND", "Project not found."));
    }

    projects.RemoveAll(p => p.Id == id);
    var updated = payload.Translations.Select(t => new ProjectItem(
        id, id, t.Lang, t.Title, t.ShortTitle ?? t.Title,
        t.ShortDescription ?? string.Empty, t.FullDescription ?? string.Empty,
        payload.ImageUrl ?? string.Empty, payload.Category ?? string.Empty,
        payload.Year ?? string.Empty, payload.Status, payload.Icon ?? string.Empty,
        DateTimeOffset.UtcNow)).ToList();
    projects.AddRange(updated);

    return Results.Ok(updated.First());
});

app.MapDelete("/api/v1/admin/content/projects/{id}", (HttpRequest request, string id) =>
{
    var authResult = EnsureAdmin(request);
    if (authResult is not null) return authResult;

    var removed = projects.RemoveAll(p => p.Id == id);
    return removed == 0
        ? Results.NotFound(ApiError("NOT_FOUND", "Project not found."))
        : Results.NoContent();
});

app.MapPost("/api/v1/admin/content/news", (HttpRequest request, NewsUpsertRequest payload) =>
{
    var authResult = EnsureAdmin(request);
    if (authResult is not null) return authResult;

    if (payload.Translations.Count == 0)
    {
        return Results.BadRequest(ApiError("VALIDATION_ERROR", "At least one translation is required."));
    }

    var created = payload.Translations.Select(t => new NewsItem(
        payload.Slug,
        payload.Slug,
        t.Lang,
        t.Title,
        t.Summary ?? string.Empty,
        t.Content ?? [],
        payload.ImageUrl ?? string.Empty,
        payload.PublishedAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty,
        payload.PublishedAt ?? DateTimeOffset.UtcNow,
        payload.Status)).ToList();

    news.RemoveAll(n => n.Id == payload.Slug);
    news.AddRange(created);

    return Results.Created($"/api/v1/content/news/{payload.Slug}", created.First());
});

app.MapPut("/api/v1/admin/content/news/{id}", (HttpRequest request, string id, NewsUpsertRequest payload) =>
{
    var authResult = EnsureAdmin(request);
    if (authResult is not null) return authResult;

    if (!news.Any(n => n.Id == id))
    {
        return Results.NotFound(ApiError("NOT_FOUND", "News item not found."));
    }

    news.RemoveAll(n => n.Id == id);
    var updated = payload.Translations.Select(t => new NewsItem(
        id,
        id,
        t.Lang,
        t.Title,
        t.Summary ?? string.Empty,
        t.Content ?? [],
        payload.ImageUrl ?? string.Empty,
        payload.PublishedAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty,
        payload.PublishedAt ?? DateTimeOffset.UtcNow,
        payload.Status)).ToList();
    news.AddRange(updated);

    return Results.Ok(updated.First());
});

app.MapDelete("/api/v1/admin/content/news/{id}", (HttpRequest request, string id) =>
{
    var authResult = EnsureAdmin(request);
    if (authResult is not null) return authResult;

    var removed = news.RemoveAll(n => n.Id == id);
    return removed == 0
        ? Results.NotFound(ApiError("NOT_FOUND", "News item not found."))
        : Results.NoContent();
});

app.Run();

static string NormalizeLang(string? lang) => string.Equals(lang, "bn", StringComparison.OrdinalIgnoreCase) ? "bn" : "en";

static IResult? EnsureAdmin(HttpRequest request)
{
    if (!request.Headers.TryGetValue("Authorization", out var authHeader))
    {
        return Results.Unauthorized();
    }

    return authHeader.ToString() == "Bearer dev-admin-token"
        ? null
        : Results.Json(ApiError("UNAUTHORIZED", "Invalid token."), statusCode: StatusCodes.Status401Unauthorized);
}

static object ApiError(string code, string message) => new { error = new { code, message } };

record ProjectItem(
    string Id,
    string Slug,
    string Lang,
    string Title,
    string ShortTitle,
    string ShortDescription,
    string FullDescription,
    string ImageUrl,
    string Category,
    string Year,
    string Status,
    string Icon,
    DateTimeOffset UpdatedAt);

record NewsItem(
    string Id,
    string Slug,
    string Lang,
    string Title,
    string Summary,
    List<string> Content,
    string ImageUrl,
    string Date,
    DateTimeOffset PublishedAt,
    string Status);

record ProjectUpsertRequest(
    string Slug,
    string? ImageUrl,
    string? Category,
    string? Year,
    string Status,
    string? Icon,
    List<ProjectTranslationInput> Translations);

record ProjectTranslationInput(string Lang, string Title, string? ShortTitle, string? ShortDescription, string? FullDescription);

record NewsUpsertRequest(
    string Slug,
    string? ImageUrl,
    DateTimeOffset? PublishedAt,
    string Status,
    List<NewsTranslationInput> Translations);

record NewsTranslationInput(string Lang, string Title, string? Summary, List<string>? Content);
