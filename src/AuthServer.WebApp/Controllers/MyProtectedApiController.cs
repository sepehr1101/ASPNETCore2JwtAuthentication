using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.WebApp.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class MyProtectedApiController : Controller
    {
        public IActionResult Get()
        {
            return Ok(new
            {
                Id = System.Guid.NewGuid(),
                Title = "Hello from My Protected Controller!",
                Username = this.User.Identity.Name
            });
        }
    }
}