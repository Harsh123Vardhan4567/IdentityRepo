using IdentityDemo.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IdentityDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataSeedingController : ControllerBase
    {
        private readonly IServiceProvider _services;
        public DataSeedingController(IServiceProvider services)
        {
            _services = services;
        }
        [HttpPost("seed-dummy-users")]
        public async Task<IActionResult> SeedDummyUsers()
        {
            await IdentityUserSeeder.SeedUsersAsync(_services);
            return Ok("Dummy users have been seeded successfully.");
        }
    }
}
