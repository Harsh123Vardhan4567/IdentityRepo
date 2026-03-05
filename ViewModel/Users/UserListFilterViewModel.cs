using System.ComponentModel.DataAnnotations;

namespace IdentityDemo.ViewModel.Users
{
    public class UserListFilterViewModel
    {

        [Display(Name = "Search")]
        [StringLength(100, ErrorMessage = "Search text cannot exceed 100 characters.")]
        public string? Search { get; set; } // Search by Email/UserName/First/Last/Phone
        [Display(Name = "Active Status")]
        public bool? IsActive { get; set; } // All/Active/Inactive
        [Display(Name = "Email Confirmation")]
        public bool? EmailConfirmed { get; set; } // All/Confirmed/Unconfirmed
        [Display(Name = "Page Number")]
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be 1 or greater.")]
        public int PageNumber { get; set; } = 1; // Default page number
        [Display(Name = "Page Size")]
        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100.")]
        public int PageSize { get; set; } = 5; // Default page size
    }
}
