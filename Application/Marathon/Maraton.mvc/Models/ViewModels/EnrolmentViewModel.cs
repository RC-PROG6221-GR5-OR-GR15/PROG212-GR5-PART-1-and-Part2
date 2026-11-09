using System.Text.Json.Serialization;

namespace Maraton.mvc.Models.ViewModels
{
    /// <summary>
    /// Mirrors the Enrolment entity the API returns, which uses snake_case
    /// property names, hence the explicit [JsonPropertyName] mappings.
    /// </summary>
    public class EnrolmentViewModel
    {
        public Guid EnrolmentId { get; set; }
        public Guid UserId { get; set; }
        public Guid EventId { get; set; }
        public Guid CategoryId { get; set; }

        /// <summary>Filled in by the controller after looking up the event/category names.</summary>
        public string EventName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;

        [JsonPropertyName("enrolment_date")]
        public DateOnly EnrolmentDate { get; set; }

        [JsonPropertyName("enrolment_status")]
        public string EnrolmentStatus { get; set; } = string.Empty;

        [JsonPropertyName("payment_status")]
        public string PaymentStatus { get; set; } = string.Empty;

        [JsonPropertyName("amount_paid")]
        public decimal AmountPaid { get; set; }

        [JsonPropertyName("paid_date")]
        public DateOnly PaidDate { get; set; }
    }
}
