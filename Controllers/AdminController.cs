using IdentityDemo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityDemo.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _useruserManager;
        public AdminController(UserManager<ApplicationUser> userManger)
        {
            _useruserManager = userManger;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
