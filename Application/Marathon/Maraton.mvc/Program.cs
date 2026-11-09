using Microsoft.AspNetCore.Authentication.Cookies;
using Maraton.mvc.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// HttpClient for API communication.
// Inside a container, localhost refers to the container itself, so the API on the
// host is reached via host.docker.internal. We keep HTTPS because the API redirects
// HTTP to HTTPS, and following that redirect would strip the Authorization header.
// The API's dev certificate is not trusted inside the container, so we bypass cert
// validation on the handler below.
var inContainer = string.Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), "true", StringComparison.OrdinalIgnoreCase);
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7233/";
if (inContainer)
{
    apiBaseUrl = apiBaseUrl.Replace("localhost", "host.docker.internal", StringComparison.OrdinalIgnoreCase);
}

builder.Services.AddHttpClient("RaceDayApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    // Dev only: the API uses a self-signed cert that the container won't trust.
    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
});

// Typed client used by the controllers so the JWT is attached consistently.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<RaceDayApiClient>(sp => new RaceDayApiClient(
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("RaceDayApi"),
    sp.GetRequiredService<IHttpContextAccessor>()));

// Authentication & Session
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "RaceDay.Session";
    });

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// HTTPS redirection is skipped inside containers that only listen on HTTP.
if (!builder.Configuration.GetValue<bool>("DisableHttpsRedirection"))
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
