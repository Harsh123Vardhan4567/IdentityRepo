using Microsoft.AspNetCore.Identity;
using System.Net;

namespace IdentityDemo.Models
{//extend the user or role entity with custom properties
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfileImage { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; }

        //Audit Columns
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }

        // Navigation property for one-to-many relationsip
        public virtual List<Address>? Addresses { get; set; }
    }
}
