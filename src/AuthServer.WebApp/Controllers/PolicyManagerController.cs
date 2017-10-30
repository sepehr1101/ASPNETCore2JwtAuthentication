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
    [EnableCors("CorsPolicy")]
     public class PolicyManagerController : BaseController
    {
         private readonly IUnitOfWork _uow;
         private readonly IPolicyService _policyService;
         private readonly IMapper _mapper;

         public PolicyManagerController(
             IMapper mapper,
             IUnitOfWork uow,
             IPolicyService policyService)
         {
             _mapper=mapper;
             _mapper.CheckArgumentIsNull(nameof(_mapper));

             _uow=uow;
             _uow.CheckArgumentIsNull(nameof(_uow));

             _policyService=policyService;
             _policyService.CheckArgumentIsNull(nameof(_policyService));
         }

        [HttpGet]
        public virtual async Task<IActionResult> GetPasswordPolicy()
        {
            var activePolicy=await _policyService.FindActiveAsync().ConfigureAwait(false);
            var passwordPolicy=_mapper.Map<PasswordPolicy>(activePolicy);
            return Ok(passwordPolicy);
        }

        [HttpPatch]
        public virtual async Task<IActionResult> UpdatePasswordPolicy([FromBody]PasswordPolicy passwordPolicy)
        {
            await _policyService.UpdatePasswordPolicy(passwordPolicy);
            await _uow.SaveChangesAsync();
            return Ok();
        }
    }
}