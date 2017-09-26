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
    [Authorize]
    public abstract class BaseController : Controller
    {
        public Guid GetMyUserId()   
        {
            var identity = User.Identity as ClaimsIdentity;
            var userIdString = identity.FindFirst(c => c.Type.Equals("userId")).Value;
            var userId = new Guid(userIdString);
            return userId;
        }
    }
}