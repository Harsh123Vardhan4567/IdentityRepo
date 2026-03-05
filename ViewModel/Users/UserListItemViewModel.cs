namespace IdentityDemo.ViewModel.Users
{
    public class UserListItemViewModel
    {
        public Guid Id { get; init; }
        public string Email { get; init; } = string.Empty;
        public string UserName { get; init; } = string.Empty;
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public string? PhoneNumber { get; init; }
        public bool IsActive { get; init; }
        public bool EmailConfirmed { get; init; }
        public DateTime? CreatedOn { get; init; }
    }
}
