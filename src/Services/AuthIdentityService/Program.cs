using AuthIdentityService.Contracts;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var users = new List<UserRecord>
{
    new(
        "usr_admin",
        "admin@site.org",
        "admin123",
        "admin",
        "active",
        new UserProfile(null, null, null, null, null, null, null, null, null, null, null, false, null, null, null, null, null, null, null))
};

var accessTokens = new Dictionary<string, UserRecord>(StringComparer.Ordinal);
var refreshTokens = new Dictionary<string, string>(StringComparer.Ordinal);

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "AuthIdentityService" }));

app.MapPost("/api/v1/auth/register", (LegacyRegistrationRequest payload) =>
{
    if (string.IsNullOrWhiteSpace(payload.Email) || string.IsNullOrWhiteSpace(payload.Password))
    {
        return Results.BadRequest(ApiError("VALIDATION_ERROR", "email and password are required."));
    }

    if (users.Any(u => string.Equals(u.Email, payload.Email, StringComparison.OrdinalIgnoreCase)))
    {
        return Results.BadRequest(ApiError("VALIDATION_ERROR", "email already exists."));
    }

    var user = LegacyRegistrationMapper.ToUserRecord(payload);
    users.Add(user);

    return Results.Created($"/api/v1/auth/users/{user.Id}", new { id = user.Id, email = user.Email, role = user.Role });
});

app.MapPost("/api/v1/auth/login", (LoginRequest payload) =>
{
    var user = users.FirstOrDefault(u => string.Equals(u.Email, payload.Email, StringComparison.OrdinalIgnoreCase) && u.Password == payload.Password && u.Status == "active");
    if (user is null)
    {
        return Results.Json(ApiError("UNAUTHORIZED", "Invalid credentials."), statusCode: StatusCodes.Status401Unauthorized);
    }

    var token = $"atk_{Guid.NewGuid():N}";
    var refresh = $"rt_{Guid.NewGuid():N}";
    accessTokens[token] = user;
    refreshTokens[refresh] = user.Id;

    return Results.Ok(new
    {
        access_token = token,
        refresh_token = refresh,
        expires_in = 900,
        token_type = "Bearer",
        user = new { id = user.Id, email = user.Email, role = user.Role }
    });
});

app.MapPost("/api/v1/auth/refresh", (RefreshTokenRequest payload) =>
{
    if (!refreshTokens.TryGetValue(payload.RefreshToken, out var userId))
    {
        return Results.Json(ApiError("UNAUTHORIZED", "Invalid refresh token."), statusCode: StatusCodes.Status401Unauthorized);
    }

    var user = users.First(u => u.Id == userId);
    var token = $"atk_{Guid.NewGuid():N}";
    var newRefresh = $"rt_{Guid.NewGuid():N}";

    refreshTokens.Remove(payload.RefreshToken);
    refreshTokens[newRefresh] = user.Id;
    accessTokens[token] = user;

    return Results.Ok(new
    {
        access_token = token,
        refresh_token = newRefresh,
        expires_in = 900,
        token_type = "Bearer",
        user = new { id = user.Id, email = user.Email, role = user.Role }
    });
});

app.MapPost("/api/v1/auth/logout", (RefreshTokenRequest payload) =>
{
    if (!refreshTokens.Remove(payload.RefreshToken))
    {
        return Results.Json(ApiError("UNAUTHORIZED", "Invalid refresh token."), statusCode: StatusCodes.Status401Unauthorized);
    }

    return Results.NoContent();
});

app.MapGet("/api/v1/auth/me", (HttpRequest request) =>
{
    if (!request.Headers.TryGetValue("Authorization", out var authHeader))
    {
        return Results.Json(ApiError("UNAUTHORIZED", "Missing Authorization header."), statusCode: StatusCodes.Status401Unauthorized);
    }

    const string bearerPrefix = "Bearer ";
    var raw = authHeader.ToString();
    if (!raw.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
    {
        return Results.Json(ApiError("UNAUTHORIZED", "Invalid Authorization header."), statusCode: StatusCodes.Status401Unauthorized);
    }

    var token = raw[bearerPrefix.Length..].Trim();
    if (!accessTokens.TryGetValue(token, out var user))
    {
        return Results.Json(ApiError("UNAUTHORIZED", "Invalid token."), statusCode: StatusCodes.Status401Unauthorized);
    }

    return Results.Ok(new { id = user.Id, email = user.Email, role = user.Role, profile = user.Profile });
});

app.Run();

static object ApiError(string code, string message) => new { error = new { code, message } };
