using System.Security.Claims;
using Maraton.mvc.Models.ViewModels;
using Maraton.mvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maraton.mvc.Controllers
{
    /// <summary>
    /// Organiser-facing area. Every action talks to the REST API through
    /// <see cref="RaceDayApiClient"/> using the JWT stored at login.
    /// </summary>
    [Authorize(Roles = "Organiser")]
    public class OrganiserController : Controller
    {
        private readonly RaceDayApiClient _api;

        public OrganiserController(RaceDayApiClient api)
        {
            _api = api;
        }

        /// <summary>UserAccountId of the signed-in organiser (matches Event.CreatedBy).</summary>
        private int CurrentUserId =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

        private Task<ApiResult<List<EventViewModel>>> AllEventsAsync() =>
            _api.GetAsync<List<EventViewModel>>("api/Events");

        private Task<ApiResult<List<EnrolmentViewModel>>> AllEnrolmentsAsync() =>
            _api.GetAsync<List<EnrolmentViewModel>>("api/Enrolment");

        // ---------------------------------------------------------------- Dashboard

        public async Task<IActionResult> Index()
        {
            var eventsResult = await AllEventsAsync();
            var events = eventsResult.Data ?? new List<EventViewModel>();
            var mine = events.Where(e => e.CreatedBy == CurrentUserId).ToList();

            var enrolmentResult = await AllEnrolmentsAsync();
            var enrolments = enrolmentResult.Data ?? new List<EnrolmentViewModel>();
            var myEventIds = mine.Select(e => e.EventId).ToHashSet();
            var myEnrolments = enrolments.Where(e => myEventIds.Contains(e.EventId)).ToList();

            var model = new OrganiserDashboardViewModel
            {
                DisplayName = User.Identity?.Name ?? "Organiser",
                ActiveEvents = mine.Count(e => !string.Equals(e.Status, "Completed", StringComparison.OrdinalIgnoreCase)),
                TotalEnrolments = myEnrolments.Count,
                PendingEnrolments = myEnrolments.Count(e =>
                    !string.Equals(e.EnrolmentStatus, "Confirmed", StringComparison.OrdinalIgnoreCase)),
                MyEvents = mine.OrderBy(e => e.EventDate).ToList()
            };

            if (!eventsResult.Success)
                ViewBag.Error = eventsResult.Error;

            return View(model);
        }

        // ------------------------------------------------------------------- Events

        public async Task<IActionResult> Events()
        {
            var result = await AllEventsAsync();
            var mine = (result.Data ?? new List<EventViewModel>())
                .Where(e => e.CreatedBy == CurrentUserId)
                .OrderBy(e => e.EventDate)
                .ToList();

            if (!result.Success) ViewBag.Error = result.Error;

            return View(mine);
        }

        [HttpGet]
        public IActionResult CreateEvent()
        {
            return View("EventForm", new EventFormViewModel());
        }

        [HttpGet]
        public async Task<IActionResult> EditEvent(Guid id)
        {
            var result = await _api.GetAsync<EventViewModel>($"api/Events/{id}");
            if (!result.Success || result.Data is null)
            {
                TempData["Error"] = result.Error ?? "That event could not be found.";
                return RedirectToAction(nameof(Events));
            }

            var model = new EventFormViewModel
            {
                EventId = result.Data.EventId,
                EventName = result.Data.EventName,
                EventDate = DateOnly.FromDateTime(result.Data.EventDate),
                Location = result.Data.Location,
                Venue = result.Data.Venue,
                DistanceKm = result.Data.DistanceKm,
                EventType = result.Data.EventType,
                EventFee = result.Data.EventFee,
                Status = result.Data.Status
            };

            return View("EventForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveEvent(EventFormViewModel model)
        {
            if (!ModelState.IsValid) return View("EventForm", model);

            var payload = new
            {
                model.EventName,
                EventDate = model.EventDate.ToString("yyyy-MM-dd"),
                model.Location,
                model.Venue,
                model.DistanceKm,
                model.EventType,
                model.EventFee,
                model.Status
            };

            ApiResult<string> result;
            if (model.IsEdit)
            {
                result = await _api.PutAsync<string>($"api/Events/{model.EventId}", payload);
            }
            else
            {
                result = await _api.PostAsync<string>("api/Events", payload);
            }

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Error ?? "The event could not be saved.");
                return View("EventForm", model);
            }

            TempData["Success"] = model.IsEdit
                ? "Event updated successfully."
                : "Event created successfully.";

            return RedirectToAction(nameof(Events));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            var result = await _api.DeleteAsync<string>($"api/Events/{id}");

            TempData[result.Success ? "Success" : "Error"] = result.Success
                ? "Event deleted successfully."
                : result.Error ?? "The event could not be deleted.";

            return RedirectToAction(nameof(Events));
        }

        // --------------------------------------------------------------- Categories

        public async Task<IActionResult> Categories(Guid id)
        {
            var eventResult = await _api.GetAsync<EventViewModel>($"api/Events/{id}");
            if (!eventResult.Success || eventResult.Data is null)
            {
                TempData["Error"] = eventResult.Error ?? "That event could not be found.";
                return RedirectToAction(nameof(Events));
            }

            var categoryResult = await _api.GetAsync<List<CategoryViewModel>>("api/Category");
            var categories = (categoryResult.Data ?? new List<CategoryViewModel>())
                .Where(c => c.EventId == id)
                .OrderBy(c => c.CategoryName)
                .ToList();

            ViewBag.EventId = eventResult.Data.EventId;
            ViewBag.EventName = eventResult.Data.EventName;

            return View(categories);
        }

        [HttpGet]
        public async Task<IActionResult> CreateCategory(Guid eventId)
        {
            var eventResult = await _api.GetAsync<EventViewModel>($"api/Events/{eventId}");
            if (!eventResult.Success || eventResult.Data is null)
            {
                TempData["Error"] = eventResult.Error ?? "That event could not be found.";
                return RedirectToAction(nameof(Events));
            }

            return View("CategoryForm", new CategoryFormViewModel
            {
                EventId = eventResult.Data.EventId,
                EventName = eventResult.Data.EventName
            });
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(Guid id)
        {
            var result = await _api.GetAsync<CategoryViewModel>($"api/Category/{id}");
            if (!result.Success || result.Data is null)
            {
                TempData["Error"] = result.Error ?? "That category could not be found.";
                return RedirectToAction(nameof(Events));
            }

            var eventResult = await _api.GetAsync<EventViewModel>($"api/Events/{result.Data.EventId}");

            return View("CategoryForm", new CategoryFormViewModel
            {
                CategoryId = result.Data.CategoryId,
                EventId = result.Data.EventId,
                EventName = eventResult.Data?.EventName ?? string.Empty,
                CategoryName = result.Data.CategoryName,
                MinAge = result.Data.MinAge ?? 0,
                MaxAge = result.Data.MaxAge ?? 99,
                GenderRestrictions = result.Data.GenderRestrictions,
                MaxParticipants = result.Data.MaxParticipants,
                EntryFee = result.Data.EntryFee
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCategory(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid) return View("CategoryForm", model);

            ApiResult<string> result;
            if (model.IsEdit)
            {
                var payload = new
                {
                    model.CategoryName,
                    model.MinAge,
                    model.MaxAge,
                    model.GenderRestrictions,
                    model.MaxParticipants,
                    model.EntryFee
                };
                result = await _api.PutAsync<string>($"api/Category/{model.CategoryId}", payload);
            }
            else
            {
                var payload = new
                {
                    model.EventId,
                    model.CategoryName,
                    model.MinAge,
                    model.MaxAge,
                    model.GenderRestrictions,
                    model.MaxParticipants,
                    model.EntryFee
                };
                result = await _api.PostAsync<string>("api/Category", payload);
            }

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Error ?? "The category could not be saved.");
                return View("CategoryForm", model);
            }

            TempData["Success"] = model.IsEdit
                ? "Category updated successfully."
                : "Category created successfully.";

            return RedirectToAction(nameof(Categories), new { id = model.EventId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(Guid id, Guid eventId)
        {
            var result = await _api.DeleteAsync<string>($"api/Category/{id}");

            TempData[result.Success ? "Success" : "Error"] = result.Success
                ? "Category deleted successfully."
                : result.Error ?? "The category could not be deleted.";

            return RedirectToAction(nameof(Categories), new { id = eventId });
        }

        // --------------------------------------------------------------- Enrolments

        public async Task<IActionResult> Enrolments(Guid id)
        {
            var eventResult = await _api.GetAsync<EventViewModel>($"api/Events/{id}");
            if (!eventResult.Success || eventResult.Data is null)
            {
                TempData["Error"] = eventResult.Error ?? "That event could not be found.";
                return RedirectToAction(nameof(Events));
            }

            var enrolmentResult = await AllEnrolmentsAsync();
            var enrolments = (enrolmentResult.Data ?? new List<EnrolmentViewModel>())
                .Where(e => e.EventId == id)
                .ToList();

            var categoryResult = await _api.GetAsync<List<CategoryViewModel>>("api/Category");
            var categoryNames = (categoryResult.Data ?? new List<CategoryViewModel>())
                .ToDictionary(c => c.CategoryId, c => c.CategoryName);

            foreach (var enrolment in enrolments)
            {
                enrolment.EventName = eventResult.Data.EventName;
                enrolment.CategoryName = categoryNames.TryGetValue(enrolment.CategoryId, out var name)
                    ? name
                    : "Unknown";
            }

            ViewBag.EventId = eventResult.Data.EventId;
            ViewBag.EventName = eventResult.Data.EventName;

            return View(enrolments.OrderBy(e => e.EnrolmentDate).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetEnrolmentStatus(Guid id, Guid eventId, string status)
        {
            var payload = new
            {
                EnrolmentStatus = status,
                PaymentStatus = string.Equals(status, "Confirmed", StringComparison.OrdinalIgnoreCase)
                    ? "Paid"
                    : "Pending"
            };

            var result = await _api.PatchAsync<string>($"api/Enrolment/{id}", payload);

            TempData[result.Success ? "Success" : "Error"] = result.Success
                ? $"Enrolment marked as {status}."
                : result.Error ?? "The enrolment could not be updated.";

            return RedirectToAction(nameof(Enrolments), new { id = eventId });
        }
    }
}
