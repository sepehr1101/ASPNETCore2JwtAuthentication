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
    public class AuthLevelsController : BaseController
    {
        private readonly IAuthLevelService _authLevelService;
        private readonly IClaimService _claimService;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public AuthLevelsController(
            IAuthLevelService authLevelService,
            IClaimService claimService,
            IMapper mapper,
            IUnitOfWork uow)
        {
            _authLevelService=authLevelService;
            _authLevelService.CheckArgumentIsNull(nameof(_authLevelService));

            _claimService=claimService;
            _claimService.CheckArgumentIsNull(nameof(_claimService));

            _mapper=mapper;
            _mapper.CheckArgumentIsNull(nameof(_mapper));

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