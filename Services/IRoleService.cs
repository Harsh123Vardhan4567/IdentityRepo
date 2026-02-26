using IdentityDemo.Models;
using IdentityDemo.ViewModel.Roles;
using Microsoft.AspNetCore.Identity;

namespace IdentityDemo.Services
{
    public interface IRoleService
    {
        Task<PageResult<RoleListItemViewModel>> GetRolesAsync(RoleListFilterViewModel filter);
        Task<(IdentityResult Result, Guid? RoleId)> CreateAsync(RoleCreateViewModel model);
        Task<RoleEditViewModel?> GetForEditAsync(Guid id);
        Task<IdentityResult> UpdateAsync(RoleEditViewModel model);
        Task<IdentityResult> DeleteAsync(Guid id);
        Task<RoleDetailsViewModel?> GetDetailsAsync(Guid id, int pageNumber, int pageSize);
    }
}
