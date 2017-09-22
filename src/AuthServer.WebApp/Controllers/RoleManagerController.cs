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
    [Authorize]
     public class RoleManagerController : Controller
    {
         private readonly IUnitOfWork _uow;
         private readonly IRolesService _roleService;

         public RoleManagerController(
             IUnitOfWork uow,
             IRolesService rolesService)
         {
             _uow=uow;
             _uow.CheckArgumentIsNull(nameof(_uow));

             _roleService=rolesService;
             _roleService.CheckArgumentIsNull(nameof(_roleService));
         }

         [HttpGet]         
         public async Task<IActionResult> GetAll()
         {
             var roles=await _roleService.GetAsync().ConfigureAwait(false);
             return Ok(roles);
         }

         [HttpPut]
         [Authorize(Policy=CustomRoles.Admin)]
         public async Task<IActionResult> Add([FromBody] RoleInfo roleInfo)
         {
            roleInfo.CheckArgumentIsNull(nameof(roleInfo));
            roleInfo.TitleEng.CheckStringIsNullOrWhiteSpace();
            roleInfo.TitleFa.CheckStringIsNullOrWhiteSpace();                 
            var role=GetRole(roleInfo);
            await _roleService.AddAsync(role);
            await _uow.SaveChangesAsync().ConfigureAwait(false);
            return Ok();
         }
         private Role GetRole(RoleInfo roleInfo)
         {
            var role=new Role();     
            role.IsActive=true;
            role.Name=roleInfo.TitleEng;
            role.TitleFa=roleInfo.TitleFa;
            return role;
         }
    }
}