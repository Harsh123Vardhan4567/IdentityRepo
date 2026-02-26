namespace IdentityDemo.ViewModel.Roles
{
    public class RoleListFilterViewModel
    {
        public string? Search { get; set; }      
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
