using Microsoft.AspNetCore.Mvc;
using Maraton.mvc.Models.ViewModels;
using System.Text.Json;
using System.Net.Http.Headers;

namespace Maraton.mvc.Controllers
{
    public class EventsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EventsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index(string type = "All")
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");
            
            // Get token from session if logged in
            var token = HttpContext.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync("api/Events");
            
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var events = JsonSerializer.Deserialize<List<EventViewModel>>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (type != "All")
                {
                    events = events.Where(e => e.EventType.Equals(type, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                ViewBag.SelectedType = type;
                return View(events);
            }

            return View(new List<EventViewModel>());
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");
            var response = await client.GetAsync($"api/Events/{id}");

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var eventItem = JsonSerializer.Deserialize<EventViewModel>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(eventItem);
            }

            return NotFound();
        }
    }
}
