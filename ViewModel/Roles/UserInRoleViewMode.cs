namespace IdentityDemo.ViewModel.Roles
{
    public class UserInRoleViewMode
    {
        public Guid Id { get; init; }
        public string Email { get; init; } = null!;
        public string? PhoneNumber { get; init; }
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public bool IsActive { get; init; }
    }
}
