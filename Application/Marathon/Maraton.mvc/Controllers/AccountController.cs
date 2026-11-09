using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Maraton.mvc.Models.ViewModels;
using System.Security.Claims;
using System.Text.Json;
using System.Text;

namespace Maraton.mvc.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient("RaceDayApi");
            var content = new StringContent(JsonSerializer.Serialize(new
            {
                Email = model.Email,
                Password = model.Password,
                Role = model.Role,
                FirstName = model.FirstName,
                LastName = model.LastName,
                DateOfBirth = model.DateOfBirth,
                PhoneNumber = model.PhoneNumber
            }), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/Auth/register", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Registration successful! Please login.";
                return RedirectToAction("Login");
            }

            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", DescribeError(error) ?? "Registration failed.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient("RaceDayApi");
            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/Auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, loginResponse.User.UserAccountId.ToString()),
                    new Claim(ClaimTypes.Name, loginResponse.User.Email),
                    new Claim(ClaimTypes.Email, loginResponse.User.Email),
                    new Claim(ClaimTypes.Role, loginResponse.User.Role),
                    new Claim("Token", loginResponse.Token)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties { IsPersistent = true };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                // Store token in session for future API calls
                HttpContext.Session.SetString("JWToken", loginResponse.Token);

                if (loginResponse.User.Role == "Organiser")
                    return RedirectToAction("Index", "Organiser");
                else
                    return RedirectToAction("Index", "Participant");
            }

            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>Pulls a readable message out of an API problem-details response.</summary>
        private static string? DescribeError(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) return null;

            try
            {
                using var document = JsonDocument.Parse(body);
                var root = document.RootElement;

                if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                {
                    foreach (var field in errors.EnumerateObject())
                    {
                        foreach (var message in field.Value.EnumerateArray())
                        {
                            return message.GetString();
                        }
                    }
                }

                if (root.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
                    return title.GetString();
            }
            catch (JsonException)
            {
                // Not JSON - fall through and return the raw body.
            }

            return body;
        }
    }

    public class LoginResponse
    {
        public string Token { get; set; }
        public UserData User { get; set; }
    }

    public class UserData
    {
        public int UserAccountId { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
