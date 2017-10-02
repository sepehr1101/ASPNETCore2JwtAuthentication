using System;
using System.Linq;
using AuthServer.Common;
using AuthServer.DataLayer.Context;
using AuthServer.DomainClasses;
using AuthServer.DomainClasses.ConstantTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuthServer.Services
{
    public interface IDbInitializerService
    {
        /// <summary>
        /// Applies any pending migrations for the context to the database.
        /// Will create the database if it does not already exist.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Adds some default values to the Db
        /// </summary>
        void SeedData();
    }

    public class DbInitializerService : IDbInitializerService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ISecurityService _securityService;

        public DbInitializerService(
            IServiceScopeFactory scopeFactory,
            ISecurityService securityService)
        {
            _scopeFactory = scopeFactory;
            _scopeFactory.CheckArgumentIsNull(nameof(_scopeFactory));

            _securityService = securityService;
            _securityService.CheckArgumentIsNull(nameof(_securityService));
        }

        public void Initialize()
        {
            using (var serviceScope = _scopeFactory.CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>())
                {
                    context.Database.Migrate();
                }
            }
        }

        public void SeedData()
        {
            using (var serviceScope = _scopeFactory.CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>())
                {
                    // Add default roles
                    var adminRole = new Role { Name = CustomRoles.Admin,TitleFa="مدیر سیستم",IsActive=true };
                    var userRole = new Role { Name = CustomRoles.User,TitleFa="کاربر",IsActive=true };
                    if (!context.Roles.Any())
                    {                       
                        context.Add(adminRole);
                        context.Add(userRole);
                        //context.SaveChanges();
                    }
                    
                    if(!context.Policies.Any())
                    {
                        var policy=new Policy
                        {
                            EnableValidIpRecaptcha=false,
                            RequireRecaptchaInvalidAttempts=6,
                            LockInvalidAttempts=10,
                            IsActive=true,
                            MinPasswordLength=4,
                            PasswordContainsLowercase=false,
                            PasswordContainsNonAlphaNumeric=false,
                            PasswordContainsNumber=false,
                            PasswordContainsUppercase=false
                        };
                        context.Add(policy);
                    }

                    // Add Admin user
                    if (!context.Users.Any())
                    {
                        var adminUser = new User
                        {
                            Id=Guid.NewGuid(),
                            Username = "sysAdmin",
                            UserCode=256,
                            DisplayName = "مدیر سیستم",
                            IsActive = true,
                            LastLoggedIn = null,
                            Password = _securityService.GetSha256Hash("123456"),
                            SerialNumber = Guid.NewGuid().ToString("N"),
                            FirstName="مدیر" ,
                            LastName="سیستم" ,
                            JoinTimespan= DateTimeOffset.UtcNow,
                            Email="sepehr@example.com",
                            EmailConfirmed=false,
                            IncludeThisRecord=true
                        };
                       
                        var adminUserRole=new UserRole { Role = adminRole, User = adminUser };
                        var userUserRole=new UserRole { Role = userRole, User = adminUser };
                        var userRoles=new UserRole[]{adminUserRole,userUserRole};
                        adminUser.UserRoles=userRoles;

                        var claim=new UserClaim{ClaimType=CustomClaimTypes.Action,
                            ClaimValue="",InsertBy=adminUser.Id,
                            InsertTimespan=DateTimeOffset.UtcNow};
                        adminUser.UserClaims=new UserClaim[]{claim};                       
                        context.Add(adminUser);                       
                    }

                    if(!context.Browsers.Any())
                    {
                        var firefox=new Browser
                        {
                            Id=1,
                            TitleEng="Mozilla FireFox",
                            TitleFa="موزیلا فایرفاکس",
                            IconClass="firefox"
                        };
                        var chrome=new Browser
                        {
                             Id=2,
                            TitleEng="Google Chrome",
                            TitleFa="گوگل کروم",
                            IconClass="chrome"
                        };
                        var edge=new Browser
                        {
                             Id=3,
                            TitleEng="Microsoft Edge",
                            TitleFa="Edge",
                            IconClass="edge"
                        };
                        var ie=new Browser
                        {
                             Id=4,
                            TitleEng="Internet Explorer",
                            TitleFa="اینترنت اکسپلورر",
                            IconClass="ie"
                        };
                        var browsers=new []{firefox,chrome,edge,ie};
                        context.AddRange(browsers);
                    }
                    context.SaveChanges();
                }
            }
        }
    }
}