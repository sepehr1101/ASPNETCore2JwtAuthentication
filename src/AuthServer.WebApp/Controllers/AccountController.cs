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
using AutoMapper;
using System.Collections.Generic;

namespace AuthServer.WebApp.Controllers
{
    [EnableCors("CorsPolicy")]
    public class AccountController : BaseController
    {
        private readonly IUsersService _usersService;
        private readonly IRolesService _roleService;
        private readonly ITokenStoreService _tokenStoreService;
        private readonly ILoginService _loginService;
        private readonly IAuthLevelService _authLevelService;
        private readonly IPolicyService _policyService;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public AccountController(
            IUsersService usersService,
            ITokenStoreService tokenStoreService,
            ILoginService loginService,
            IAuthLevelService authLevelService,
            IPolicyService policyService,
            IRolesService rolesService,
            IUnitOfWork uow,
            IMapper mapper)
        {
            _usersService = usersService;
            _usersService.CheckArgumentIsNull(nameof(usersService));

            _tokenStoreService = tokenStoreService;
            _tokenStoreService.CheckArgumentIsNull(nameof(_tokenStoreService));

            _loginService=loginService;
            _loginService.CheckArgumentIsNull(nameof(_loginService));

            _authLevelService=authLevelService;
            _authLevelService.CheckArgumentIsNull(nameof(_authLevelService));

            _policyService=policyService;
            _policyService.CheckArgumentIsNull(nameof(_policyService));

            _roleService=rolesService;
            _roleService.CheckArgumentIsNull(nameof(_roleService));

            _uow = uow;
            _uow.CheckArgumentIsNull(nameof(_uow));

            _mapper=mapper;
            _mapper.CheckArgumentIsNull(nameof(_mapper));
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody]  LoginInfo loginUser)
        {       
            bool canILogin=false;
            if (!ModelState.IsValid)
            {
                return BadRequest("لطفا داده های ورودی خود را کنترل فرمایید");
            }         
            var user = await _usersService.FindUserAsync(loginUser.Username, loginUser.Password).ConfigureAwait(false);
            if(user==null)
            {
                user=await _usersService.FindUserAsync(loginUser.Username).ConfigureAwait(false);
                if(user==null)
                {
                    return Unauthorized();
                }
                var activePolicy=await _policyService.FindActiveAsync();
                _usersService.FailedLoginAttempt(user,activePolicy);
                var loginFailure = GetLogin(false,user);
                _loginService.Add(loginFailure);
                await _uow.SaveChangesAsync().ConfigureAwait(false);
                return Unauthorized();
            }
            canILogin= user != null && user.IsActive;
            if (!canILogin)
            {
                return Unauthorized();
            }
            if(!_usersService.CanILogin(user))
            {
                return BadRequest("به دلیل وارد کردن یوزر و پسوورد اشتباه نام کاربری شما قفل شده است");
            }
            if(await _roleService.IsDeviceIdRequired(user.Id))
            {
                if(!_usersService.CanMathDeviceId(user,loginUser))
                {
                    return BadRequest("لطفا ورودی های خود را کنترل فرمایید");
                }
            }
            var accessList=await _authLevelService.GetMyAccessListAsync(user.Id);
            var (accessToken, refreshToken) = await _tokenStoreService.CreateJwtTokens(user).ConfigureAwait(false);
            var loginSuccess = GetLogin(true,user);            
            _loginService.Add(loginSuccess);
            _usersService.SuccessLoginAttempt(user);
            await _uow.SaveChangesAsync().ConfigureAwait(false);
            return Ok(new { access_token = accessToken, refresh_token = refreshToken,accessList=accessList });
        }

        [HttpGet]
        public async Task<IActionResult> GetUserLogins(Guid userId)
        {
            var userLogins=await _loginService.GetUserLogins(userId);
            var userLoginsDto=_mapper.Map<List<LoginViewModel>>(userLogins);
            return Ok(userLoginsDto);
        }
        private Login GetLogin(bool canILogin,User user)
        {
            var login=new Login();
            var userAgent=Request.Headers["User-Agent"]; 
            UserAgent.UserAgent ua = new UserAgent.UserAgent(userAgent); 
            login.BrowserId=ua.Browser.BrowserCode;
            login.BrowserVersion=ua.Browser.Version;
            login.BrowserTitle=ua.Browser.Name;
            login.Id=Guid.NewGuid();
            login.LoginIP=HttpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress?.ToString();
            login.LoginTimespan=DateTimeOffset.UtcNow;
            login.User=user;
            login.OsVersion=ua.OS.Version;
            login.OsTitle=ua.OS.Name;
            login.WasSuccessful=canILogin;

            return login;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> RefreshToken([FromBody]JToken jsonBody)
        {
            var refreshToken = jsonBody.Value<string>("refreshToken");
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return BadRequest("refreshToken is not set.");
            }

            var token = await _tokenStoreService.FindTokenAsync(refreshToken);
            if (token == null)
            {
                return Unauthorized();
            }

            var (accessToken, newRefreshToken) = await _tokenStoreService.CreateJwtTokens(token.User).ConfigureAwait(false);
            return Ok(new { access_token = accessToken, refresh_token = newRefreshToken });
        }
       
        [HttpGet, HttpPost]
        public async Task<bool> Logout()
        {
            var userId=GetMyUserId();

            // The Jwt implementation does not support "revoke OAuth token" (logout) by design.
            // Delete the user's tokens from the database (revoke its bearer token)          
            await _tokenStoreService.InvalidateUserTokensAsync(userId).ConfigureAwait(false);
                        
            await _tokenStoreService.DeleteExpiredTokensAsync().ConfigureAwait(false);
            await _uow.SaveChangesAsync().ConfigureAwait(false);

            return true;
        }
              
        [HttpGet("[action]"), HttpPost("[action]")]
        public IActionResult GetUserInfo()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            return Json(new { Username = claimsIdentity.Name });
        }

        [HttpPatch]
        public async Task<IActionResult> UpdateDeviceId(string deviceId)
        {
            var policy=await _policyService.FindFirstAsync();            
            var simpleMessage=new SimpleMessageResponse();
            if(!policy.CanUpdateDeviceId)
            {
                simpleMessage.Message="امکان به روز رسانی سریال دستگاه وجود ندارد";
                return BadRequest(simpleMessage);
            }
            var userId=GetMyUserId();
            await _usersService.UpdateDeviceSerialAsync(userId,deviceId)
                .ConfigureAwait(false);
            await _uow.SaveChangesAsync().ConfigureAwait(false);
            simpleMessage.Message="تغییر یا ثبت سریال دستگاه با موفقیت انجام شد";
            return Ok(simpleMessage);
        }
         [HttpPatch]
         [AllowAnonymous]
        public async Task<IActionResult> UpdateDeviceIdAnanymous([FromBody]  LoginInfo loginUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("لطفا داده های ورودی خود را کنترل فرمایید");
            }        
             var user = await _usersService.FindUserAsync(loginUser.Username, loginUser.Password).ConfigureAwait(false);
            if(user==null)
            {
                return Unauthorized();
            }
            var policy=await _policyService.FindActiveAsync();            
            var simpleMessage=new SimpleMessageResponse();
            if(!policy.CanUpdateDeviceId)
            {
                simpleMessage.Message="امکان به روز رسانی سریال دستگاه وجود ندارد";
                return BadRequest(simpleMessage);
            }  
            await _usersService.UpdateDeviceSerialAsync(user.Id,loginUser.DeviceId)
                .ConfigureAwait(false);
            await _uow.SaveChangesAsync().ConfigureAwait(false);
            simpleMessage.Message="تغییر یا ثبت سریال دستگاه با موفقیت انجام شد";
            return Ok(simpleMessage);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(Guid userId)
        {
            await _usersService.ResetPasswordAsync(userId);
            await _uow.SaveChangesAsync();
            return Ok("ریست کلمه عبور کاربر با موفقیت انجام شد");
        }
    }
}