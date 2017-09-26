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
using AutoMapper;

namespace AuthServer.WebApp.Controllers
{
    [Route("[controller]")]
    [EnableCors("CorsPolicy")]
    //[Authorize]
     public class UserManagerController : BaseController
    {
         private readonly IUnitOfWork _uow;
         private readonly IUsersService _userService;
         private readonly IClaimService _claimService;
         private readonly IRolesService _roleService;
         private readonly IMapper _mapper;

         public UserManagerController(
             IMapper mapper,
             IUnitOfWork uow,
             IUsersService usersService,
             IClaimService claimService,
             IRolesService rolesService)
         {
             _mapper=mapper;
             _mapper.CheckArgumentIsNull(nameof(_mapper));

             _uow=uow;
             _uow.CheckArgumentIsNull(nameof(_uow));

             _userService=usersService;
             _userService.CheckArgumentIsNull(nameof(_userService));

             _claimService=claimService;
             _claimService.CheckArgumentIsNull(nameof(_claimService));

             _roleService=rolesService;
             _roleService.CheckArgumentIsNull(nameof(_roleService));
         }

         [HttpPut]    
         public async Task<IActionResult> RegisterUser([FromBody]RegisterUserViewModel registerUserViewModel)
         {
             if(!ModelState.IsValid)
             {
                 return BadRequest("خطا در اطلاعات");
             }
             var user=GetUser(registerUserViewModel);
             await _userService.RegisterUserAsync(user).ConfigureAwait(false);
             await _uow.SaveChangesAsync().ConfigureAwait(false);
             return Ok(registerUserViewModel);
         }
         private User GetUser(RegisterUserViewModel registerUserViewModel)
         {
             var userId=GetMyUserId();
             var user=Mapper.Map<User>(registerUserViewModel);
             user.Id=Guid.NewGuid();
             user.IsActive=true;
             user.JoinTimespan=DateTime.UtcNow;
             user.SerialNumber=Guid.NewGuid().ToString("N");
             user.UserClaims=GetClaims(registerUserViewModel.ZoneIds,registerUserViewModel.ClaimValues,userId);
             user.UserRoles=_roleService.ConvertToUserRoles(registerUserViewModel.RoleIds);
             return user;                     
         }
         private ICollection<UserClaim> GetClaims(ICollection<string> zoneIds,ICollection<string> actions,Guid userId)
         {
             var zoneIdClaims=_claimService.ConvertToClaims("zoneId",zoneIds,userId);
             var actionClaims=_claimService.ConvertToClaims("action",actions,userId);
             if(zoneIdClaims!=null && actionClaims!=null)
             {
                 zoneIdClaims.AddRange(actionClaims);
                 return zoneIdClaims;
             }
             if(zoneIdClaims!=null && actionClaims==null)
             {
                 return zoneIdClaims;
             }
             if(zoneIdClaims==null && actionClaims!=null)
             {
                 return actionClaims;
             }
             throw new ArgumentNullException("user without role and zone!");
         }
    }
}