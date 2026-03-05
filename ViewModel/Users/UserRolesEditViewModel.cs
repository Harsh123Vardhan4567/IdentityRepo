using System.ComponentModel.DataAnnotations;

namespace IdentityDemo.ViewModel.Users
{
    public class UserRolesEditViewModel
    {
        [Required(ErrorMessage = "Invalid user.")]
        public Guid UserId { get; set; }
        [Display(Name = "User Name")]
        public string UserName { get; set; } = string.Empty;
        // Roles collection can be empty (no roles selected), so no Required here.
        public List<RoleCheckboxItem> Roles { get; set; } = new();
    }
}
