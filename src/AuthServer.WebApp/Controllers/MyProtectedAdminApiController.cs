using System.Security.Claims;
using AuthServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuthServer.Common;
using System.Threading.Tasks;
using System;

namespace AuthServer.WebApp.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Policy = CustomRoles.Admin)]
    public class MyProtectedAdminApiController : Controller
    {
        private readonly IUsersService _usersService;

        public MyProtectedAdminApiController(IUsersService usersService)
        {
            _usersService = usersService;
            _usersService.CheckArgumentIsNull(nameof(usersService));
        }

        public async Task<IActionResult> Get()
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var userDataClaim = claimsIdentity.FindFirst(ClaimTypes.UserData);
            var userId = userDataClaim.Value;

            return Ok(new
            {
                Id = Guid.NewGuid(),
                Title = "Hello from My Protected Admin Api Controller!",
                Username = this.User.Identity.Name,
                UserData = userId,
                TokenSerialNumber = await _usersService.GetSerialNumberAsync(Guid.Parse(userId)).ConfigureAwait(false)
            });
        }
    }
}