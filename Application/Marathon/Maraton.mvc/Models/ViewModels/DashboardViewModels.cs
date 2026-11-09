namespace Maraton.mvc.Models.ViewModels
{
    /// <summary>Aggregated figures shown on the Organiser landing page.</summary>
    public class OrganiserDashboardViewModel
    {
        public string DisplayName { get; set; } = string.Empty;

        public int ActiveEvents { get; set; }
        public int TotalEnrolments { get; set; }
        public int PendingEnrolments { get; set; }

        public List<EventViewModel> MyEvents { get; set; } = new();
    }

    /// <summary>Aggregated figures shown on the Participant landing page.</summary>
    public class ParticipantDashboardViewModel
    {
        public string DisplayName { get; set; } = string.Empty;

        public int TotalEnrolments { get; set; }
        public int ConfirmedEnrolments { get; set; }
        public int PendingEnrolments { get; set; }

        public List<EnrolmentViewModel> MyEnrolments { get; set; } = new();
    }
}
