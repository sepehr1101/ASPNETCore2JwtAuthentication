using System;
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
     public class RoleUserManagerController : Controller
    {
         private readonly IUnitOfWork _uow;
         private readonly IRolesService _roleService;

         public RoleUserManagerController(
             IUnitOfWork uow,
             IRolesService rolesService)
         {
             _uow=uow;
             _uow.CheckArgumentIsNull(nameof(_uow));

             _roleService=rolesService;
             _roleService.CheckArgumentIsNull(nameof(_roleService));
         }

         [HttpGet]         
         public async Task<IActionResult> GetUsersInRole(int roleId)
         {
             var roles=await _roleService.FindUsersInRoleAsync(roleId).ConfigureAwait(false);
             return Ok(roles);
         }

        /* [HttpPut]
         public async Task<IActionResult> AssignRoleToUser(Guid userId,int roleId)
         {
             var role=_roleService.GetAsync(roleId);
             role.CheckArgumentIsNull(nameof(role));
             
         }*/
       
    }
}