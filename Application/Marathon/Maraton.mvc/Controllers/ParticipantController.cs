using System.Security.Claims;
using Maraton.mvc.Models.ViewModels;
using Maraton.mvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maraton.mvc.Controllers
{
    /// <summary>
    /// Participant-facing area: personal enrolments and the enrolment flow.
    /// </summary>
    [Authorize(Roles = "Participant")]
    public class ParticipantController : Controller
    {
        private readonly RaceDayApiClient _api;

        public ParticipantController(RaceDayApiClient api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _api.GetAsync<List<EnrolmentViewModel>>("api/Enrolment/me");
            var enrolments = result.Data ?? new List<EnrolmentViewModel>();

            await EnrichAsync(enrolments);

            var model = new ParticipantDashboardViewModel
            {
                DisplayName = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name ?? "Participant",
                TotalEnrolments = enrolments.Count,
                ConfirmedEnrolments = enrolments.Count(IsConfirmed),
                PendingEnrolments = enrolments.Count(e => !IsConfirmed(e)),
                MyEnrolments = enrolments.OrderByDescending(e => e.EnrolmentDate).ToList()
            };

            if (!result.Success) ViewBag.Error = result.Error;

            return View(model);
        }

        /// <summary>Category picker for a single event.</summary>
        [HttpGet]
        public async Task<IActionResult> Enrol(Guid id)
        {
            var eventResult = await _api.GetAsync<EventViewModel>($"api/Events/{id}");
            if (!eventResult.Success || eventResult.Data is null)
            {
                TempData["Error"] = eventResult.Error ?? "That event could not be found.";
                return RedirectToAction("Index", "Events");
            }

            var categoryResult = await _api.GetAsync<List<CategoryViewModel>>("api/Category");
            var categories = (categoryResult.Data ?? new List<CategoryViewModel>())
                .Where(c => c.EventId == id)
                .OrderBy(c => c.CategoryName)
                .ToList();

            // Hide categories the participant is already enrolled in.
            var mine = await _api.GetAsync<List<EnrolmentViewModel>>("api/Enrolment/me");
            var takenCategoryIds = (mine.Data ?? new List<EnrolmentViewModel>())
                .Select(e => e.CategoryId)
                .ToHashSet();

            ViewBag.Event = eventResult.Data;
            ViewBag.TakenCount = categories.Count(c => takenCategoryIds.Contains(c.CategoryId));

            return View(categories.Where(c => !takenCategoryIds.Contains(c.CategoryId)).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enrol(Guid eventId, Guid categoryId, decimal amountPaid)
        {
            if (amountPaid < 0)
            {
                TempData["Error"] = "Amount paid cannot be negative.";
                return RedirectToAction(nameof(Enrol), new { id = eventId });
            }

            var result = await _api.PostAsync<string>("api/Enrolment", new
            {
                CategoryId = categoryId,
                AmountPaid = amountPaid
            });

            if (!result.Success)
            {
                TempData["Error"] = result.Error ?? "The enrolment could not be created.";
                return RedirectToAction(nameof(Enrol), new { id = eventId });
            }

            TempData["Success"] = "You have been enrolled. Track your status on your dashboard.";
            return RedirectToAction(nameof(Index));
        }

        private static bool IsConfirmed(EnrolmentViewModel enrolment) =>
            string.Equals(enrolment.EnrolmentStatus, "Confirmed", StringComparison.OrdinalIgnoreCase);

        /// <summary>Fills in the display names the API does not return on enrolments.</summary>
        private async Task EnrichAsync(List<EnrolmentViewModel> enrolments)
        {
            if (enrolments.Count == 0) return;

            var events = await _api.GetAsync<List<EventViewModel>>("api/Events");
            var eventsById = (events.Data ?? new List<EventViewModel>()).ToDictionary(e => e.EventId);

            var categories = await _api.GetAsync<List<CategoryViewModel>>("api/Category");
            var categoriesById = (categories.Data ?? new List<CategoryViewModel>()).ToDictionary(c => c.CategoryId);

            foreach (var enrolment in enrolments)
            {
                enrolment.EventName = eventsById.TryGetValue(enrolment.EventId, out var ev)
                    ? ev.EventName
                    : "Unknown event";

                enrolment.CategoryName = categoriesById.TryGetValue(enrolment.CategoryId, out var cat)
                    ? cat.CategoryName
                    : "Unknown category";
            }
        }
    }
}
