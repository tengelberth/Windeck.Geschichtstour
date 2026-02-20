using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Windeck.Geschichtstour.Backend.Data;
using Windeck.Geschichtstour.Backend.Services;


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
builder.Services.AddControllers();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

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

    // XML-Kommentare (wenn aktiviert, siehe csproj)
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (System.IO.File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// ----------------------------------------
// Datenbank migrieren und Startinhalte anlegen
// ----------------------------------------
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Stellt sicher, dass alle Migrationen angewendet sind.
    dbContext.Database.Migrate();

    // Startinhalte einspielen, falls noch keine Stationen existieren.
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
}).ExcludeFromDescription(); // nicht in Swagger anzeigen

// Routing aktivieren.
app.UseRouting();

// WICHTIG: zuerst Authentication, dann Authorization
app.UseAuthentication();
app.UseAuthorization();

// Fallback für Deeplink-URLs im Browser:
// Wenn jemand /station?code=XYZ öffnet, leite auf /Index weiter (optional: Query behalten)
app.MapGet("/station", (HttpContext ctx) =>
{
    // Optional: Code mitgeben, falls der Wert auf der Startseite weiterverarbeitet werden soll
    //var code = ctx.Request.Query["code"].ToString();

    // Variante ohne Query-Weitergabe:
    // return Results.Redirect("/Index", permanent: false);

    // Variante mit Query-Weitergabe:
    //if (!string.IsNullOrWhiteSpace(code))
    //    return Results.Redirect($"/Index?code={Uri.EscapeDataString(code)}", permanent: false);

    return Results.Redirect("/Index", permanent: false);
}).ExcludeFromDescription(); // nicht in Swagger anzeigen

app.MapGet("/share/station", (HttpContext ctx) =>
{
    // Optional: Code mitgeben, falls der Wert auf der Startseite weiterverarbeitet werden soll
    //var code = ctx.Request.Query["code"].ToString();

    // Variante ohne Query-Weitergabe:
    // return Results.Redirect("/Index", permanent: false);

    // Variante mit Query-Weitergabe:
    //if (!string.IsNullOrWhiteSpace(code))
    //    return Results.Redirect($"/Index?code={Uri.EscapeDataString(code)}", permanent: false);

    return Results.Redirect("/Index", permanent: false);
}).ExcludeFromDescription(); // nicht in Swagger anzeigen

// Razor Pages (Adminoberfläche) unter Standardrouten verfügbar machen.
app.MapRazorPages();

// API-Controller unter /api/... verfügbar machen.
app.MapControllers();

// Anwendung starten.
app.Run();



