using System.ComponentModel.DataAnnotations;

namespace Maraton.mvc.Models.ViewModels
{
    /// <summary>Used by the Organiser for both creating and editing a category.</summary>
    public class CategoryFormViewModel
    {
        /// <summary>Null when creating a new category.</summary>
        public Guid? CategoryId { get; set; }

        public bool IsEdit => CategoryId.HasValue;

        [Required]
        public Guid EventId { get; set; }

        [Display(Name = "Event")]
        public string EventName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter a category name.")]
        [StringLength(80)]
        [Display(Name = "Category name")]
        public string CategoryName { get; set; } = string.Empty;

        [Required]
        [Range(0, 120, ErrorMessage = "Minimum age must be between 0 and 120.")]
        [Display(Name = "Minimum age")]
        public int MinAge { get; set; }

        [Required]
        [Range(0, 120, ErrorMessage = "Maximum age must be between 0 and 120.")]
        [Display(Name = "Maximum age")]
        public int MaxAge { get; set; } = 99;

        [Required(ErrorMessage = "Please specify the gender restriction.")]
        [StringLength(40)]
        [Display(Name = "Gender restrictions")]
        public string GenderRestrictions { get; set; } = "Open";

        [Required]
        [Range(1, 100000, ErrorMessage = "Maximum participants must be at least 1.")]
        [Display(Name = "Maximum participants")]
        public int MaxParticipants { get; set; } = 100;

        [Required]
        [Range(0, 100000, ErrorMessage = "Entry fee must be between R0 and R100 000.")]
        [Display(Name = "Entry fee (R)")]
        public decimal EntryFee { get; set; }
    }
}
