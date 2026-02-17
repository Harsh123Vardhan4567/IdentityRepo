using Microsoft.AspNetCore.Identity;

namespace IdentityDemo.Models
{
    public class ApplicationRoles:IdentityRole<Guid>
    {
        // Extended property
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        //Audit Columns
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
