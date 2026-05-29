using AuthIdentityService.Contracts;
using AuthIdentityService.Data;
using AuthIdentityService.Data.Entities;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("AuthDb") ?? "Data Source=auth.db"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    db.Database.Migrate();

    if (!db.Users.Any(u => u.Email == "admin@site.org"))
    {
        db.Users.Add(new AuthUserEntity
        {
            Id = "usr_admin",
            Email = "admin@site.org",
            Password = "admin123",
            Role = "admin",
            Status = "active",
            CreatedAt = DateTimeOffset.UtcNow
        });
        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var accessTokens = new Dictionary<string, string>(StringComparer.Ordinal);

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "AuthIdentityService" }));

app.MapPost("/api/v1/auth/register", async (LegacyRegistrationRequest payload, AuthDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(payload.Email) || string.IsNullOrWhiteSpace(payload.Password))
    {
        return Results.BadRequest(ApiError("VALIDATION_ERROR", "email and password are required."));
    }

    var email = payload.Email.Trim().ToLowerInvariant();
    if (await db.Users.AnyAsync(u => u.Email == email))
    {
        return Results.BadRequest(ApiError("VALIDATION_ERROR", "email already exists."));
    }

    var role = string.IsNullOrWhiteSpace(payload.Roles) ? "editor" : payload.Roles.Split(',')[0].Trim().ToLowerInvariant();
    if (role is not ("admin" or "editor")) role = "editor";

    var user = new AuthUserEntity
    {
        Id = $"usr_{Guid.NewGuid():N}",
        Email = email,
        Password = payload.Password,
        Role = role,
        Status = "active",
        CreatedAt = DateTimeOffset.UtcNow,
        FirstName = payload.FirstName,
        LastName = payload.LastName,
        Gender = payload.Gender,
        ReasonForJoining = payload.ReasonForJoining,
        PresentOrganization = payload.PresentOrganization,
        VolunteeingExperience = payload.VolunteeingExperience,
        DateOfBirth = payload.DateOfBirth,
        CityOfResidence = payload.CityOfResidence,
        CountryOfResidence = payload.CountryOfResidence,
        PermanentAddress = payload.PermanentAddress,
        MailingAddress = payload.MailingAddress,
        IsMailingAddressSameAsPermanentAddress = payload.IsMailingAddressSameAsPermanentAddress ?? false,
        BloodGroup = payload.BloodGroup,
        AreasOfExpertise = payload.AreasOfExpertise,
        HighestDegree = payload.HighestDegree,
        DisabilitiesIfAny = payload.DisabilitiesIfAny,
        Nationality = payload.Nationality,
        PersonalWebPage = payload.PersonalWebPage,
        SocialMediaLink = payload.SocialMediaLink
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Created($"/api/v1/auth/users/{user.Id}", new { id = user.Id, email = user.Email, role = user.Role });
});

app.MapPost("/api/v1/auth/login", async (LoginRequest payload, AuthDbContext db) =>
{
    var email = payload.Email.Trim().ToLowerInvariant();
    var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email && u.Password == payload.Password && u.Status == "active");
    if (user is null)
    {
        return Results.Json(ApiError("UNAUTHORIZED", "Invalid credentials."), statusCode: StatusCodes.Status401Unauthorized);
    }

    var token = $"atk_{Guid.NewGuid():N}";
    var refresh = $"rt_{Guid.NewGuid():N}";
    accessTokens[token] = user.Id;

    db.RefreshTokens.Add(new RefreshTokenEntity
    {
        Id = $"rft_{Guid.NewGuid():N}",
        Token = refresh,
        UserId = user.Id,
        CreatedAt = DateTimeOffset.UtcNow,
        ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
    });
    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        access_token = token,
        refresh_token = refresh,
        expires_in = 900,
        token_type = "Bearer",
        user = new { id = user.Id, email = user.Email, role = user.Role }
    });
});

app.MapPost("/api/v1/auth/refresh", async (RefreshTokenRequest payload, AuthDbContext db) =>
{
    var refreshRow = await db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == payload.RefreshToken && r.RevokedAt == null && r.ExpiresAt > DateTimeOffset.UtcNow);
    if (refreshRow is null)
    {
        return Results.Json(ApiError("UNAUTHORIZED", "Invalid refresh token."), statusCode: StatusCodes.Status401Unauthorized);
    }

    var user = await db.Users.FirstOrDefaultAsync(u => u.Id == refreshRow.UserId && u.Status == "active");
    if (user is null)
    {
        return Results.Json(ApiError("UNAUTHORIZED", "Invalid refresh token."), statusCode: StatusCodes.Status401Unauthorized);
    }

    refreshRow.RevokedAt = DateTimeOffset.UtcNow;

    var token = $"atk_{Guid.NewGuid():N}";
    var newRefresh = $"rt_{Guid.NewGuid():N}";
    accessTokens[token] = user.Id;

    db.RefreshTokens.Add(new RefreshTokenEntity
    {
        Id = $"rft_{Guid.NewGuid():N}",
        Token = newRefresh,
        UserId = user.Id,
        CreatedAt = DateTimeOffset.UtcNow,
        ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
    });
    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        access_token = token,
        refresh_token = newRefresh,
        expires_in = 900,
        token_type = "Bearer",
        user = new { id = user.Id, email = user.Email, role = user.Role }
    });
});

app.MapPost("/api/v1/auth/logout", async (RefreshTokenRequest payload, AuthDbContext db) =>
{
    var refreshRow = await db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == payload.RefreshToken && r.RevokedAt == null);
    if (refreshRow is null)
    {
        return Results.Json(ApiError("UNAUTHORIZED", "Invalid refresh token."), statusCode: StatusCodes.Status401Unauthorized);
    }

    refreshRow.RevokedAt = DateTimeOffset.UtcNow;
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapGet("/api/v1/auth/me", async (HttpRequest request, AuthDbContext db) =>
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
    if (!accessTokens.TryGetValue(token, out var userId))
    {
        return Results.Json(ApiError("UNAUTHORIZED", "Invalid token."), statusCode: StatusCodes.Status401Unauthorized);
    }

    var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
    if (user is null)
    {
        return Results.Json(ApiError("UNAUTHORIZED", "Invalid token."), statusCode: StatusCodes.Status401Unauthorized);
    }

    return Results.Ok(new
    {
        id = user.Id,
        email = user.Email,
        role = user.Role,
        profile = new
        {
            user.FirstName,
            user.LastName,
            user.Gender,
            user.ReasonForJoining,
            user.PresentOrganization,
            user.VolunteeingExperience,
            user.DateOfBirth,
            user.CityOfResidence,
            user.CountryOfResidence,
            user.PermanentAddress,
            user.MailingAddress,
            user.IsMailingAddressSameAsPermanentAddress,
            user.BloodGroup,
            user.AreasOfExpertise,
            user.HighestDegree,
            user.DisabilitiesIfAny,
            user.Nationality,
            user.PersonalWebPage,
            user.SocialMediaLink
        }
    });
});

app.Run();

static object ApiError(string code, string message) => new { error = new { code, message } };
