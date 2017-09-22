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
    public class AccountController : Controller
    {
        private readonly IUsersService _usersService;
        private readonly ITokenStoreService _tokenStoreService;
        private readonly ILoginService _loginService;
        private readonly IUnitOfWork _uow;

        public AccountController(
            IUsersService usersService,
            ITokenStoreService tokenStoreService,
            ILoginService loginService,
            IUnitOfWork uow)
        {
            _usersService = usersService;
            _usersService.CheckArgumentIsNull(nameof(usersService));

            _tokenStoreService = tokenStoreService;
            _tokenStoreService.CheckArgumentIsNull(nameof(_tokenStoreService));

            _loginService=loginService;
            _loginService.CheckArgumentIsNull(nameof(_loginService));

            _uow = uow;
            _uow.CheckArgumentIsNull(nameof(_uow));
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> Login([FromBody]  LoginInfo loginUser)
        {
            bool canILogin=false;
            if (loginUser == null)
            {
                return BadRequest("user is not set.");
            }         
            var user = await _usersService.FindUserAsync(loginUser.Username, loginUser.Password).ConfigureAwait(false);
            if(user==null)
            {
                user=await _usersService.FindUserAsync(loginUser.Username).ConfigureAwait(false);
                if(user==null)
                {
                    return Unauthorized();
                }
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

            var (accessToken, refreshToken) = await _tokenStoreService.CreateJwtTokens(user).ConfigureAwait(false);
            var loginSuccess = GetLogin(canILogin,user);
            _loginService.Add(loginSuccess);
            await _uow.SaveChangesAsync().ConfigureAwait(false);
            return Ok(new { access_token = accessToken, refresh_token = refreshToken });
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
        [HttpPost("[action]")]
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

        [AllowAnonymous]
        [HttpGet("[action]"), HttpPost("[action]")]
        public async Task<bool> Logout()
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var userIdValue = claimsIdentity.FindFirst(ClaimTypes.UserData)?.Value;

            // The Jwt implementation does not support "revoke OAuth token" (logout) by design.
            // Delete the user's tokens from the database (revoke its bearer token)
            if (!string.IsNullOrWhiteSpace(userIdValue) && System.Guid.TryParse(userIdValue, out System.Guid userId))
            {
                await _tokenStoreService.InvalidateUserTokensAsync(userId).ConfigureAwait(false);
            }
            await _tokenStoreService.DeleteExpiredTokensAsync().ConfigureAwait(false);
            await _uow.SaveChangesAsync().ConfigureAwait(false);

            return true;
        }

        [HttpGet("[action]"), HttpPost("[action]")]
        public bool IsAuthenthenticated()
        {
            return User.Identity.IsAuthenticated;
        }

        [HttpGet("[action]"), HttpPost("[action]")]
        public IActionResult GetUserInfo()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            return Json(new { Username = claimsIdentity.Name });
        }
    }
}