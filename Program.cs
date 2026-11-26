using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Portfolio.Repositories;
using Portfolio.Security;

var builder = WebApplication.CreateBuilder(args);

// ---- Database: one NpgsqlDataSource (connection pool) shared by every repository ----
var connectionString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException(
        "Missing connection string 'ConnectionStrings:Default'. See README.md, step 3 (user-secrets or environment variable).");
builder.Services.AddSingleton(NpgsqlDataSource.Create(connectionString));

// ---- One repository class per table, registered here ----
builder.Services.AddScoped<CourseRepository>();
builder.Services.AddScoped<ProjectImageRepository>();
builder.Services.AddScoped<ProjectRepository>();

// ---- Editor sign-in (extra credit): cookie authentication + credentials from configuration ----
builder.Services.AddSingleton<EditorCredentialChecker>();
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";          // signed-out visitors are redirected here
        options.AccessDeniedPath = "/Account/Login";
        options.Cookie.Name = "PortfolioEditor";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();

// ---- MVC. Every POST must carry an anti-forgery token (CSRF protection) ----
builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

var app = builder.Build();

// ---- Friendly pages: unexpected exception -> /Error ; 404 (and other codes) -> /Error/{code} ----
app.UseExceptionHandler("/Error");
app.UseStatusCodePagesWithReExecute("/Error/{0}");
if (!app.Environment.IsDevelopment()) app.UseHsts();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();   // who is the visitor?
app.UseAuthorization();    // [Authorize] on EditorBaseController blocks signed-out visitors

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
