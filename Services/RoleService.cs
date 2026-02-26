using ASPNETCoreIdentityDemo.Data;
using IdentityDemo.Models;
using IdentityDemo.ViewModel.Roles;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityDemo.Services
{
    public class RoleService : IRoleService
    {

        private readonly RoleManager<ApplicationRoles> _roleManager;
        private readonly ApplicationDbContext _dbContext;
        public RoleService(RoleManager<ApplicationRoles> roleManager, ApplicationDbContext dbContext)
        {
                _roleManager = roleManager;
            _dbContext = dbContext; 
        }
        public async Task<PageResult<RoleListItemViewModel>> GetRolesAsync(RoleListFilterViewModel filter)
        {
            // Start with all roles (No Tracking = better performance for read-only queries)
            var query = _roleManager.Roles.AsNoTracking();
            // Apply search filter (if provided)
            if (!string.IsNullOrWhiteSpace(filter.Search))
                
             {
                var s = filter.Search.Trim();
                query = query.Where(r => r.Name!.Contains(s) || (r.Description ?? "").Contains(s));
            }
            // Apply Active/Inactive filter (if provided)
            if (filter.IsActive.HasValue)
                query = query.Where(r => r.IsActive == filter.IsActive.Value);
            // Get total role count for pagination
            var total = await query.CountAsync();
            // Get current page of roles
            var items = await query
                .OrderBy(r => r.Name) 
                .Skip((filter.PageNumber - 1) * filter.PageSize) 
                .Select(r => new RoleListItemViewModel
                {
                    Id = r.Id,
                    Name = r.Name!,
                    
                    Description = r.Description,
                    IsActive = r.IsActive,
                    CreatedOn = r.CreatedOn
                })
                .ToListAsync();
            // Return paginated result
            return new PageResult<RoleListItemViewModel>
            {
                Items = items,
                TotalCount = total,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }
       
        public async Task<(IdentityResult Result, Guid? RoleId)> CreateAsync(RoleCreateViewModel model)
        {
            
            bool roleExists = await _roleManager.RoleExistsAsync(model.Name);
         
            if (roleExists)
            {
                return (IdentityResult.Failed(new IdentityError { Description = "Role name already exists." }), null);
            }
           
            var role = new ApplicationRoles
            {
                Id = Guid.NewGuid(),
                Name = model.Name.Trim(),
                NormalizedName = model.Name.Trim().ToUpperInvariant(), 
                IsActive = model.IsActive,
                CreatedOn = DateTime.UtcNow,
                ModifiedOn = DateTime.UtcNow
            };
            // Save to database
            var result = await _roleManager.CreateAsync(role);
            return (result, result.Succeeded ? role.Id : null);
            
        }
        // Retrieves a role for editing.
        public async Task<RoleEditViewModel?> GetForEditAsync(Guid id)
        {
            var role = await _roleManager.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
            if (role == null)
                return null;
            // Map to edit view model
            return new RoleEditViewModel
            {
                Id = role.Id,
                Name = role.Name ?? string.Empty,
                Description = role.Description,
                IsActive = role.IsActive,
                ConcurrencyStamp = role.ConcurrencyStamp // For concurrency checks
            };
        }
        // Updates an existing role.	
       
        public async Task<IdentityResult> UpdateAsync(RoleEditViewModel model)
        {
            var role = await _roleManager.FindByIdAsync(model.Id.ToString());
            if (role == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "NotFound",
                    Description = "Role not found."
                });
            }
            // Concurrency check — prevents overwriting changes made by others
            if (!string.Equals(role.ConcurrencyStamp, model.ConcurrencyStamp, StringComparison.Ordinal))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "ConcurrencyFailure",
                    Description = "This role was modified by another user while you were editing. Please reload the page and try again."
                });
                
            }
            // Ensure name is still unique (excluding current role)
            if (!string.Equals(role.Name, model.Name, StringComparison.Ordinal))
            {
                var dup = await _roleManager.FindByNameAsync(model.Name);
                if (dup != null && dup.Id != role.Id)
                {
                    return IdentityResult.Failed(new IdentityError
                    {
                        Code = "DuplicateRoleName",
                        Description = $"Another role already uses this name: {model.Name}"
                    });
                }
            }
            // Update properties
            role.Name = model.Name.Trim();
            role.NormalizedName = role.Name.ToUpperInvariant();
            role.Description = model.Description;
            role.IsActive = model.IsActive;
        
            role.ModifiedOn = DateTime.UtcNow;
            // Save changes — updates ConcurrencyStamp automatically
            return await _roleManager.UpdateAsync(role);
        }
        // Deletes a role if it has no assigned users.
        public async Task<IdentityResult> DeleteAsync(Guid id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
                return IdentityResult.Failed(new IdentityError { Description = "Role not found." });
            // Prevent deletion if any users are assigned to this role
            var hasUsers = await _dbContext.Set<IdentityUserRole<Guid>>()
                .AsNoTracking()
                .AnyAsync(ur => ur.RoleId == id);
            if (hasUsers)
                return IdentityResult.Failed(new IdentityError { Description = "Cannot delete a role that still has users. Remove users from the role first." });
            
            // Delete role
            return await _roleManager.DeleteAsync(role);
        }
        // Retrieves details of a role, including paginated list of users in that role.
        public async Task<RoleDetailsViewModel?> GetDetailsAsync(Guid id, int pageNumber, int pageSize)
        {
            var role = await _roleManager.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
            if (role == null)
                return null;
            // Query all users in this role via junction table (IdentityUserRole)
            var usersQuery =
                from ur in _dbContext.Set<IdentityUserRole<Guid>>().AsNoTracking() //Left table - User Roles
                join u in _dbContext.Set<ApplicationUser>().AsNoTracking() //Right table - Users
                on ur.UserId equals u.Id
                where ur.RoleId == id

                select new UserInRoleViewMode
                {
                    Id = u.Id,
                    Email = u.Email!,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    IsActive = u.IsActive,
                    PhoneNumber = u.PhoneNumber
                };
            // Get total user count
            var total = await usersQuery.CountAsync();
            // Get current page of users
            var users = await usersQuery
                .OrderBy(u => u.Email)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            // Return role details with users
            return new RoleDetailsViewModel

            {
                Id = role.Id,
                Name = role.Name ?? string.Empty,
                Description = role.Description,
                IsActive = role.IsActive,
                CreatedOn = role.CreatedOn,
                ModifiedOn = role.ModifiedOn,
                Users = new PageResult<UserInRoleViewMode>
                {
                    Items = users,
                    TotalCount = total,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                }
            }
            ;
        }
    }
}
