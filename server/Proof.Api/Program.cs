using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Proof.Api.Data;
using Proof.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    // Enums serialize as their name ("Positive"), not the underlying int
    // (0) — self-documenting JSON instead of a magic number clients would
    // have to look up in source. Safe to add now: nothing yet consumes an
    // int-serialized enum body (Season's existing use is query-string only,
    // which ASP.NET Core binds by name regardless of this setting).
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddDbContext<ProofDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ProofDb")));
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        // AllowCredentials is required for the browser to send/receive the
        // httpOnly refresh-token cookie across the 5173/5168 port split —
        // only works paired with an explicit origin (WithOrigins), never
        // with a wildcard, which is why this couldn't just be AllowAnyOrigin.
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<DataSeedService>();
builder.Services.AddScoped<IngredientFlavorTagSyncService>();
builder.Services.AddScoped<CocktailFlavorTagSyncService>();
builder.Services.AddScoped<IngredientSpiritSyncService>();
builder.Services.AddScoped<TasteRankingService>();
builder.Services.AddScoped<IngredientSubstitutionSeedService>();
builder.Services.AddScoped<WhatCanIMakeService>();
builder.Services.AddHttpClient<CocktailDbSyncService>(client =>
{
    client.BaseAddress = new Uri("https://www.thecocktaildb.com/api/json/v1/1/");
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Without this, ASP.NET Core silently renames short claim names (like "sub")
        // into long legacy URIs behind the scenes, so User.FindFirst("sub") would
        // mysteriously come back empty even though the token clearly has it.
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SigningKey"]!)),
            // With MapInboundClaims off (above), ASP.NET Core won't automatically
            // recognize our short "role" claim as the one [Authorize(Roles = "Admin")]
            // should check — this says explicitly which claim type that is.
            RoleClaimType = "role"
        };
    });
builder.Services.AddAuthorization();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Seed lookup data (Spirits, FlavorTags) on startup if it's not already there.
// DbContext is "scoped" (one instance per HTTP request), but this code runs
// before any requests exist, so there's no request to hang a scope off of —
// CreateScope() manually creates one just for this startup work.
using (var scope = app.Services.CreateScope())
{
    var seedService = scope.ServiceProvider.GetRequiredService<DataSeedService>();
    await seedService.SeedAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AllowClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
