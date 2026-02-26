using IdentityDemo.Services;
using IdentityDemo.ViewModel.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityDemo.Controllers
{
    [Authorize]
    public class RolesController : Controller
    {
        private readonly IRoleService _roleService;
        private readonly ILogger<RolesController> _logger;
        public RolesController(IRoleService roleService, ILogger<RolesController> logger)
        {
            _roleService = roleService;
            _logger = logger;
        }
        // GET: /Roles
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] RoleListFilterViewModel filter)
        {
            try
            {
                var result = await _roleService.GetRolesAsync(filter);
                ViewBag.Filter = filter;
                return View(result);
            }
            catch (Exception ex)


            {
                _logger.LogError(ex, "Error fetching roles in Index action.");
                return View("Error");
            }
        }
        // GET: /Roles/Create
        [HttpGet]
        public IActionResult Create()
        {
            try
            {
                return View(new RoleCreateViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering Create Role form.");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleCreateViewModel model)
        {
            try
            {
                // DataAnnotations validation first
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                var (result, id) = await _roleService.CreateAsync(model);
                if (result.Succeeded)
                {
                    TempData["Success"] = $"Role '{model.Name}' created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                // Map IdentityResult errors to MODEL-LEVEL errors
                foreach (var e in result.Errors)
                   

                {
                    ModelState.AddModelError(string.Empty, e.Description);
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role '{RoleName}'.", model?.Name);
                return View("Error");
            }
        }
        // GET: /Roles/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var vm = await _roleService.GetForEditAsync(id);
                if (vm == null) return NotFound();
              

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching role for edit. RoleId: {RoleId}", id);
                return View("Error");
            }
        }
        // POST: /Roles/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoleEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
               

                var result = await _roleService.UpdateAsync(model);
                if (result.Succeeded)
                {
                    TempData["Success"] = $"Role '{model.Name}' updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                // Map IdentityResult errors to MODEL-LEVEL errors
                foreach (var e in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, e.Description);
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role '{RoleName}'.", model?.Name);
                return View("Error");
                

            }
        }
        // GET: /Roles/Details/{id}?page=1&pageSize=4
        [HttpGet]
        public async Task<IActionResult> Details(Guid id, int pageNumber = 1, int pageSize = 4)
        {
            try
            {
                var vm = await _roleService.GetDetailsAsync(id, pageNumber, pageSize);
                if (vm == null) return NotFound();
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching role details. RoleId: {RoleId}", id);
                return View("Error");
            }
        }
       

                // POST: /Roles/Delete/{id}
                [HttpPost]
                [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _roleService.DeleteAsync(id);
                if (result.Succeeded)
                {
                    TempData["Success"] = "Role deleted.";
                }
                else
                {
                    TempData["Error"] = string.Join(" ", result.Errors.Select(e => e.Description));
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role. RoleId: {RoleId}", id);
                return View("Error");
            }
        }
    }
}
