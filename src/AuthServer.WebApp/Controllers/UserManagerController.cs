using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthServer.Common;
using AuthServer.DataLayer.Context;
using AuthServer.DomainClasses;
using AuthServer.DomainClasses.ViewModels;
using AuthServer.DomainClasses.ConstantTypes;
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
    [EnableCors("CorsPolicy")]
     public class UserManagerController : BaseController
    {
         private readonly IUnitOfWork _uow;
         private readonly IUsersService _userService;
         private readonly IClaimService _claimService;
         private readonly IRolesService _roleService;
         private readonly IAuthLevelService _authLevelService;
         private readonly IPasswordValidatorService _passwordValidator;
         private readonly IMapper _mapper;

         public UserManagerController(
             IMapper mapper,
             IUnitOfWork uow,
             IUsersService usersService,
             IClaimService claimService,
             IRolesService rolesService,
             IAuthLevelService authLevelService,
             IPasswordValidatorService passwordValidator)
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

             _authLevelService=authLevelService;
             _authLevelService.CheckArgumentIsNull(nameof(_authLevelService));

             _passwordValidator=passwordValidator;
             _passwordValidator.CheckArgumentIsNull(nameof(_passwordValidator));
         }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers(int take=500,int skip=0)
        {
            var users=await _userService.Get(take,skip).ConfigureAwait(false);
            var usersDisplayViewModel=_mapper.Map<List<UserDisplayViewModel>>(users);
            return Ok(usersDisplayViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserEditInfo(Guid id)
        {
            var userAuthTree=await _authLevelService.GetUserAccessListAsync(id)
                .ConfigureAwait(false);
            var roleInfos= await _roleService.GetUserRoleInfoAsync(id).ConfigureAwait(false);
            var user=await _userService.FindUserAsync(id).ConfigureAwait(false);
            var userInfo=_mapper.Map<UserInfo>(user);
            var userEditInfo=new UserEditViewModel(roleInfos,userAuthTree,userInfo);
            return Ok(userEditInfo);
        }        
        
        [HttpPut]    
        public async Task<IActionResult> RegisterUser([FromBody]RegisterUserViewModel registerUserViewModel)
        {
            var simpleMessageCode=new SimpleMessageCodeResponse();
            string _errorMessage=String.Empty;
            if(!ModelState.IsValid)
            {                 
                simpleMessageCode.Code=400;
                simpleMessageCode.Message="خطا در اطلاعات ارسالی ، لطفا ورودی های خود را کنترل فرمایید";
                return Ok(simpleMessageCode);
            }
            var passwordValidationError=_passwordValidator.ValidatePassword(registerUserViewModel.Password);
            if(passwordValidationError.HasError)
            {
                simpleMessageCode.Code=400;
                simpleMessageCode.Message=(String)passwordValidationError.Error;
                return Ok(simpleMessageCode);
            }
            var user = GetUser(registerUserViewModel);
            var errorInfo=await GetUserRegisterError(user);
            if(errorInfo.HasError)
            {
                simpleMessageCode.Code=400;
                simpleMessageCode.Message=(String)errorInfo.Error;
                return Ok(simpleMessageCode);
            }      
            await _userService.RegisterUserAsync(user).ConfigureAwait(false);
            await _uow.SaveChangesAsync().ConfigureAwait(false);

            simpleMessageCode.Code=200;
            simpleMessageCode.Message=String.Join(" ","کاربر","با کد کاربری",registerUserViewModel.UserCode,"و نام ",
                registerUserViewModel.DisplayName,"با موفقیت ثبت شد");
            //var simpleMessageresponse=new SimpleMessageResponse(successMessage);
            return Ok(simpleMessageCode);
        }

        [HttpPatch]
        public async Task<IActionResult> UpdateUser([FromBody]UpdateUserViewModel updateUserViewModel)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest("خطا در اطلاعات ، لطفا ورودی های خود را کنترل فرمایید");
            }
            var myUserId=GetMyUserId();
            var userInDb=await _userService.FindUserAsync(updateUserViewModel.UserId).ConfigureAwait(false);
            await _roleService.DisablePreviuosRoles(userInDb.Id);
            await _claimService.DisablePrviousClaims(userInDb.Id);
            var userRoles=_roleService.ConvertToUserRoles(updateUserViewModel.RoleIds,updateUserViewModel.UserId);
            var userClaims=GetClaims(updateUserViewModel.ZoneIds,updateUserViewModel.Actions,myUserId,updateUserViewModel.UserId);
            await _claimService.AddRangeAsync(userClaims);
            await _roleService.AddRangeAsync(userRoles);
            _userService.UpdateUserAsync(userInDb,updateUserViewModel);
            await _uow.SaveChangesAsync();
            var successMessage=String.Join(" ","اطلاعات",updateUserViewModel.DisplayName,"با موفقیت ویرایش شد");
            return Ok(successMessage);
        }

        [HttpPatch]
        public async Task<IActionResult> ChangePassword([FromBody]ChangePasswordViewModel changePasswordViewModel)
        {
            var simpleMessageresponse=new SimpleMessageResponse();
            if(!ModelState.IsValid)
            {
                simpleMessageresponse.Message="لطفا ورودی های خود را کنترل فرمایید";
                return BadRequest(simpleMessageresponse);
            }
             var passwordValidationError=_passwordValidator.ValidatePassword(changePasswordViewModel.NewPassword);
            if(passwordValidationError.HasError)
            {
                return BadRequest(passwordValidationError.Error);
            }
            var username=GetMyUsername();
            var user= await _userService.FindUserAsync(username,changePasswordViewModel.CurrentPassword);
            if(user==null)
            {
                simpleMessageresponse.Message="پسوورد قبلی را اشتباه وارد نموده اید";
                return BadRequest(simpleMessageresponse);
            }
            _userService.ChangeMyPassword(user,changePasswordViewModel.NewPassword);
            await _uow.SaveChangesAsync();
            simpleMessageresponse.Message="پسوورد با موفقیت تغییر یافت";
            return Ok(simpleMessageresponse);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserClaims(Guid id)
        {
            var userClaims=await _claimService.GetUserClaimsAsync(id).ConfigureAwait(false);            
            var userClaimViewModels=_mapper.Map<List<UserClaimViewModel>>(userClaims);
            return Ok(userClaimViewModels);
        }

        [HttpPost]
        public async Task<IActionResult> GetUsersPro([FromBody]UserSearchProViewModel userSearchProViewModel)
        {
            var claimQuery=_claimService.AddClaimsToQuery(userSearchProViewModel.Claims);
            var roleQuery=_roleService.AddRolesToQuery(userSearchProViewModel.RoleIds);
            var users=await _userService.FindUsersAsync(claimQuery,roleQuery).ConfigureAwait(false);
            var usersDisplayViewModel=_mapper.Map<List<UserDisplayViewModel>>(users);
            return Ok(usersDisplayViewModel);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetUsersInRoleZone(int roleId,int zoneId)
        {
            var userClaimsQuery=_claimService.GetUserClaimsQuery(CustomClaimTypes.ZoneId,zoneId.ToString());
            var userRoleQuery = _roleService.GetUserRolesQuery(roleId);
            var users=await _userService.FindUsersAsync(userClaimsQuery,userRoleQuery);
            var usersValueKeys =_mapper.Map<List<UserValueKey>>(users);
            return Ok(usersValueKeys);
        }      
        
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetUsersInRole(int roleId)
        {
            var userRoleQuery = _roleService.GetUserRolesQuery(roleId);
            var users= await _userService.FindUsersAsync(userRoleQuery);
            var usersValueKeys =_mapper.Map<List<UserValueKey>>(users);
            return Ok(usersValueKeys);
        }

        [HttpGet,HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GetUsersByCode(ICollection<int> userCodes)
        {
            var usersCodesTitle= await _userService.GetUsersCodeTitle(userCodes);
            return Ok(usersCodesTitle);
        }

        [HttpGet,HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserByUserId(Guid id)
        {
            var user= await _userService.FindUserAsync(id);
            var userInfo=new {UserCode=user.UserCode,DisplayName=user.DisplayName,Mobile=user.Mobile};
            return Ok(userInfo);
        }

        #region private methods (3)
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

         private async Task<ErrorInfo> GetUserRegisterError(User user)
         {
             ErrorInfo errorInfo=new ErrorInfo();
            if(await _userService.CanFindUserAsync(user.UserCode))
            {
                errorInfo.HasError=true;
                errorInfo.Error=String.Join(" ","کاربر با کد کاربری",user.UserCode,"قبلا ثبت شده است");
                return errorInfo;
            }
            if(await _userService.CanFindUserAsync(user.LowercaseUsername))
            { 
                errorInfo.HasError=true;
                errorInfo.Error=String.Join(" ","کاربر با نام کاربری",user.Username,"قبلا ثبت شده است");
                 return errorInfo;
            }
            if(await _userService.CanFindUserAsync(user.LowercaseEmail))
            {
                errorInfo.HasError=true;
                errorInfo.Error=String.Join(" ","کاربر با ایمیل",user.Email,"قبلا ثبت شده است");
                return errorInfo;
            }
            errorInfo.HasError=false;
            return errorInfo;
         }
         #endregion
    }
}