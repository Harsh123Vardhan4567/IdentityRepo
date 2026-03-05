namespace IdentityDemo.ViewModel.Users
{
    public class UserDetailsViewModel
    {
        public Guid Id { get; init; }
        public string Email { get; init; } = string.Empty;
        public string UserName { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string? LastName { get; init; }
        public string? PhoneNumber { get; init; }
        public DateTime? DateOfBirth { get; init; }
        public DateTime? LastLogin { get; init; }
        public bool IsActive { get; init; }
        public bool EmailConfirmed { get; init; }
        public DateTime? CreatedOn { get; init; }
        public DateTime? ModifiedOn { get; init; }
        public List<string> Roles { get; init; } = new();
    }
}
