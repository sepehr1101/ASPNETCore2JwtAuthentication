using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthServer.Common;
using AuthServer.DataLayer.Context;
using AuthServer.DomainClasses;
using AuthServer.DomainClasses.ViewModels;
using AuthServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features;
using Newtonsoft.Json.Linq;

namespace AuthServer.WebApp.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("CorsPolicy")]
    //[Authorize]
     public class UserManagerController : Controller
    {
         private readonly IUnitOfWork _uow;
         private readonly IUsersService _userService;

         public UserManagerController(
             IUnitOfWork uow,
             IUsersService usersService)
         {
             _uow=uow;
             _uow.CheckArgumentIsNull(nameof(_uow));

             _userService=usersService;
             _userService.CheckArgumentIsNull(nameof(_userService));
         }

         [HttpGet]         
         public async Task<IActionResult> RegisterUser(RegisterViewModel userViewModel,
            ICollection<string> selectedRoles, ICollection<string> zoneIds, ICollection<string> selectedActions)
         {
             var users=await _roleService.FindUsersInRoleAsync(roleId).ConfigureAwait(false);
             return Ok(roles);
         }
    }
}