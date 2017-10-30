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
    [EnableCors("CorsPolicy")]
     public class RoleUserManagerController : BaseController
    {
         private readonly IUnitOfWork _uow;
         private readonly IRolesService _roleService;
         private readonly IPasswordValidatorService _passwordValidator;

         public RoleUserManagerController(
             IUnitOfWork uow,
             IRolesService rolesService,
             IPasswordValidatorService passwordValidator)
         {
             _uow=uow;
             _uow.CheckArgumentIsNull(nameof(_uow));

             _roleService=rolesService;
             _roleService.CheckArgumentIsNull(nameof(_roleService));

             _passwordValidator=passwordValidator;
         }

         [HttpGet]         
         public async Task<IActionResult> GetUsersInRole(int roleId)
         {
             var roles=await _roleService.FindUsersInRoleAsync(roleId).ConfigureAwait(false);
             return Ok(roles);
         }

         [HttpGet]
         [AllowAnonymous]
         public IActionResult Test()
         {
             var passwordValidation1=_passwordValidator.ValidatePassword("123");
             var passwordValidation2=_passwordValidator.ValidatePassword("abcd2");
             var passwordValidation3=_passwordValidator.ValidatePassword("ABCD3");
             var passwordValidation4=_passwordValidator.ValidatePassword("Abc3$");
             return Ok();
         }
    }
}