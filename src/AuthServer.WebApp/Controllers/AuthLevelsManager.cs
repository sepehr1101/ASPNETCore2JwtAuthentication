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
    public class AuthLevelsController : Controller
    {
        private readonly IAuthLevelService _authLevelService;
        private readonly IUnitOfWork _uow;

        public AuthLevelsController(
            IAuthLevelService authLevelService,
            IUnitOfWork uow)
        {
            _authLevelService=authLevelService;
            _authLevelService.CheckArgumentIsNull(nameof(_authLevelService));

            _uow = uow;
            _uow.CheckArgumentIsNull(nameof(_uow));
        }

        [HttpGet]
        public async Task<IActionResult> GetAuthLevels()
        {
            var authTree=await _authLevelService.GetAuthLevelsAsync();
            return Ok(authTree);
        }
    }
}