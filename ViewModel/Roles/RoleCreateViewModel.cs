using System.ComponentModel.DataAnnotations;

namespace IdentityDemo.ViewModel.Roles
{
    public class RoleCreateViewModel
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(256, ErrorMessage = "Name cannot exceed 256 characters.")]
        public string Name { get; set; } = null!;
        [StringLength(512, ErrorMessage = "Description cannot exceed 512 characters.")]
        public string? Description { get; set; }
        [Display(Name = "Active?")]
        public bool IsActive { get; set; } = true;
    }
}
