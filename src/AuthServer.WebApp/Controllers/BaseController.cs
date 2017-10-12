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
        private ClaimsIdentity GetMyClaimsIdentity()
        {
            var identity = User.Identity as ClaimsIdentity;
            return identity;
        }
        public Guid GetMyUserId()   
        {
            var identity = GetMyClaimsIdentity();
            var userIdString = identity.FindFirst(c => c.Type.Equals("userId")).Value;
            var userId = new Guid(userIdString);
            return userId;
        }

        public bool TokenContainsThis(string controller,string action)
        {
            var shouldFindValue=String.Join(".",controller.Trim(),action.Trim());
            var identity = GetMyClaimsIdentity();
            var actionValue=identity.FindFirst(c =>
                 c.Type =="action" &&
                 c.Value==shouldFindValue);
            if(actionValue==null)
            {
                return false;
            }
            return true;
        }
    }
}