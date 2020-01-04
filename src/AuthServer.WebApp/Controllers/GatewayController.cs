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
using ElmahCore;
using Microsoft.Web.Administration;

namespace AuthServer.WebApp.Controllers
{
    [EnableCors("CorsPolicy")]
     public class GatewayController : BaseController
    {        
         public GatewayController()
         {
             
         }
         
         [HttpGet]
         public IActionResult ProtectMe(string controller1,string action1)
         {
             var amIValid= TokenContainsThis(controller1,action1);
             if(amIValid)
             {
                 return Ok();
             }
             var exceptionMessage=String.Join(": ","username",GetMyUsername(),"controller",controller1,"action",action1);
             HttpContext.RiseError(new UnauthorizedAccessException(exceptionMessage));
             return Unauthorized();
         }

         [AllowAnonymous]
         public IActionResult MakeException()
         {
             HttpContext.RiseError(new Exception("test exception"));
             return Unauthorized();
         }

         [AllowAnonymous]
          public IActionResult ResetIIS()
          {
            var yourAppPool=new ServerManager().ApplicationPools["DefaultAppPool"];
            if(yourAppPool!=null)
            yourAppPool.Start();
            return Content("IIS Restart Done");
          }
    }
}