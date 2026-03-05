using System.ComponentModel.DataAnnotations;

namespace IdentityDemo.ViewModel.Users
{
    public class RoleCheckboxItem
    {
        [Required(ErrorMessage = "Role Id is required.")]
        public Guid RoleId { get; set; }
        [Required(ErrorMessage = "Role name is required.")]
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsSelected { get; set; }
    }
}
