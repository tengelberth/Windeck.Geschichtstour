using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Windeck.Geschichtstour.Backend.Data;


var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------------
// Dienste registrieren (Dependency Injection Container)
// --------------------------------------------------------

// Razor Pages mit Admin-Ordner absichern
builder.Services.AddRazorPages(options =>
{
    // Ganzen /Admin-Ordner nur für angemeldete Nutzer freigeben
    options.Conventions.AuthorizeFolder("/Admin");

    // Login-Seite explizit anonym erlauben
    options.Conventions.AllowAnonymousToPage("/Account/Login");

    // Landing Page (Startseite) explizit anonym erlauben
    options.Conventions.AllowAnonymousToPage("/Index");
});

// Entity Framework Core DbContext mit SQL Server.
// Nutzt den Connection String aus appsettings.json ("DefaultConnection").
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlOptions => sqlOptions.EnableRetryOnFailure(
        maxRetryCount: 5,
        maxRetryDelay: TimeSpan.FromSeconds(10),
        errorNumbersToAdd: null)));


// Web-API-Controller für die mobile App.
// (Die Controller fügen wir später hinzu.)
builder.Services.AddControllers();

// Cookie-Authentifizierung hinzufügen
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Wohin wird umgeleitet, wenn man nicht eingeloggt ist
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
    });

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Windeck Geschichtstour API",
        Version = "v1",
        Description = "API für Stationen und Touren der digitalen Geschichtstour Windeck."
    });

    // XML-Kommentare (wenn aktiviert, s. csproj unten)
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (System.IO.File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// ----------------------------------------
// Datenbank migrieren & Seed-Daten anlegen
// ----------------------------------------
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Stellt sicher, dass alle Migrationen angewendet sind.
    dbContext.Database.Migrate();

    // Beispiel-Daten einspielen, falls noch keine Stationen existieren.
    SeedData.Initialize(dbContext);
}

// --------------------------------------------------------
// HTTP-Pipeline konfigurieren
// --------------------------------------------------------

// Swagger-UI im Entwicklungsmodus + Produktiv aktivieren.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Windeck Geschichtstour API v1");
});

// HTTPS-Umleitung aktivieren.
app.UseHttpsRedirection();

// Statische Dateien aus wwwroot ausliefern (z. B. CSS, JS, Medien).
app.UseStaticFiles();

// AASA explizit ausliefern (weil keine Dateiendung)
app.MapGet("/.well-known/apple-app-site-association", (IWebHostEnvironment env) =>
{
    var filePath = Path.Combine(env.WebRootPath, ".well-known", "apple-app-site-association");
    return Results.File(filePath, "application/json");
});

// Routing aktivieren.
app.UseRouting();

// WICHTIG: zuerst Authentication, dann Authorization
app.UseAuthentication();
app.UseAuthorization();

// Razor Pages (Adminoberfläche) unter Standardrouten verfügbar machen.
app.MapRazorPages();

// API-Controller unter /api/... verfügbar machen.
app.MapControllers();

// Anwendung starten.
app.Run();
