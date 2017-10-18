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
using System.Transactions;

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

        [HttpGet]
        public async Task<IActionResult> GetAllUsers(int take=200,int skip=0)
        {
            var users=await _userService.Get(take,skip).ConfigureAwait(false);
            var usersDisplayViewModel=_mapper.Map<List<UserDisplayViewModel>>(users);
            return Ok(usersDisplayViewModel);
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

         [HttpPatch]
         public async Task<IActionResult> UpdateUser([FromBody]UpdateUserViewModel updateUserViewModel)
         {
             if(!ModelState.IsValid)
             {
                 return BadRequest("خطا در اطلاعات");
             }
            var myUserId=GetMyUserId();
            var userInDb=await _userService.FindUserAsync(updateUserViewModel.Id).ConfigureAwait(false);
            await _roleService.DisablePreviuosRoles(userInDb.Id);
            await _claimService.DisablePrviousClaims(userInDb.Id);
            var userRoles=_roleService.ConvertToUserRoles(updateUserViewModel.RoleIds,updateUserViewModel.Id);
            var userClaims=GetClaims(updateUserViewModel.ZoneIds,updateUserViewModel.Actions,myUserId,updateUserViewModel.Id);
            await _claimService.AddRangeAsync(userClaims);
            await _roleService.AddRangeAsync(userRoles);
            await _uow.SaveChangesAsync();
            return Ok();
         }
         private User GetUser(RegisterUserViewModel registerUserViewModel)
         {
             var myUserId=GetMyUserId();
             var user=Mapper.Map<User>(registerUserViewModel);
             user.Id=Guid.NewGuid();
             user.IsActive=true;
             user.JoinTimespan=DateTime.UtcNow;
             user.SerialNumber=Guid.NewGuid().ToString("N");
             user.UserClaims=GetClaims(registerUserViewModel.ZoneIds,registerUserViewModel.Actions,myUserId);
             user.UserRoles=_roleService.ConvertToUserRoles(registerUserViewModel.RoleIds);
             user.LowercaseEmail=user.Email.ToLower();
             user.LowercaseUsername=user.Username.ToLower();
             user.Mobile=registerUserViewModel.Mobile;
             user.DeviceId=registerUserViewModel.deviceId;
             return user;                     
         }
         private ICollection<UserClaim> GetClaims(ICollection<string> zoneIds,ICollection<string> actions,Guid insertBy,Guid? userId=null)
         {
             var zoneIdClaims=userId.HasValue? _claimService.ConvertToClaims("zoneId",zoneIds,insertBy,userId.Value):
                _claimService.ConvertToClaims("zoneId",zoneIds,insertBy);
             var actionClaims=userId.HasValue ?  _claimService.ConvertToClaims("action",actions,insertBy,userId.Value):
                 _claimService.ConvertToClaims("action",actions,insertBy);
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