using IdentityDemo.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityDemo.Data
{
    public class IdentityUserSeeder
    {
        private const string DefaultPassword = "Test@1234";


        private static readonly Dictionary<string, List<(string FirstName, string LastName, string Phone, DateTime DOB)>> RoleUsers =
            new()
            {
                ["Admin"] = new List<(string, string, string, DateTime)>
                {
                    ("Pranav", "Sharma", "9876543210", new DateTime(1985, 5, 12)),
                    ("Aditi", "Verma", "9123456789", new DateTime(1990, 3, 8)),
                    ("Rohan", "Iyer", "9988776655", new DateTime(1988, 7, 21)),
                    ("Neha", "Chopra", "9012345678", new DateTime(1992, 11, 15)),
                    ("Kunal", "Reddy", "9765432109", new DateTime(1986, 9, 30))
                },
                ["Manager"] = new List<(string, string, string, DateTime)>
                {
                    ("Arjun", "Mehta", "9823456780", new DateTime(1984, 4, 17)),
                    ("Sneha", "Kapoor", "9345678901", new DateTime(1991, 6, 9)),
                    ("Vikram", "Patel", "9456789012", new DateTime(1989, 1, 25)),
                    ("Pooja", "Nair", "9567890123", new DateTime(1993, 8, 14)),
                    ("Anil", "Deshmukh", "9678901234", new DateTime(1987, 2, 19))
                },
                ["User"] = new List<(string, string, string, DateTime)>
                {
                    ("Rahul", "Singh", "9789012345", new DateTime(1994, 12, 4)),
                    ("Priya", "Menon", "9890123456", new DateTime(1995, 10, 22)),
                    ("Siddharth", "Joshi", "9901234567", new DateTime(1992, 9, 18)),
                    ("Ananya", "Pillai", "9812345678", new DateTime(1996, 5, 5)),
                    ("Manoj", "Bose", "9923456789", new DateTime(1991, 3, 29))
                }
            };

        public static async Task SeedUsersAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            foreach (var role in RoleUsers.Keys)
            {
                foreach (var (firstName, lastName, phone, dob) in RoleUsers[role])
                {
                    var email = $"{firstName.ToLower()}.{lastName.ToLower()}@dotnettutorials.net";
                    await EnsureUserInRoleAsync(userManager, firstName, lastName, email, phone, dob, role, DefaultPassword);
                }
            }
        }

        private static async Task EnsureUserInRoleAsync(
            UserManager<ApplicationUser> userManager,
            string firstName,
            string lastName,
            string email,
            string phone,
            DateTime dob,
            string role,
            string password)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = email,
                    NormalizedUserName = email.ToUpperInvariant(),
                    Email = email,
                    NormalizedEmail = email.ToUpperInvariant(),
                    EmailConfirmed = true,
                    FirstName = firstName,
                    LastName = lastName,
                    PhoneNumber = phone,
                    DateOfBirth = dob, // Ensure this property exists in ApplicationUser
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow,
                    ModifiedOn = DateTime.UtcNow
                };
                var createResult = await userManager.CreateAsync(user, password);
                if (!createResult.Succeeded)
                {
                    // Optional: log error
                    return;
                }
            }
            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }

    }
}
