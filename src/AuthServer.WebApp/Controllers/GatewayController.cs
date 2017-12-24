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
             return Unauthorized();
         }
    }
}