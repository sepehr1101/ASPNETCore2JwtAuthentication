using System;
using System.Linq;
using AuthServer.Common;
using AuthServer.DataLayer.Context;
using AuthServer.DomainClasses;
using AuthServer.DomainClasses.ConstantTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

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
                        var policy= CreatePolicy();
                        context.Add(policy);
                    }

                    if(!context.AuthLevel1s.Any())
                    {
                        var query=GetAuthLevelQuery();
                        context.Database.ExecuteSqlCommand(query);
                    }
                    // Add Admin user
                    if (!context.Users.Any())
                    {
                        var adminUser = CreateAdminUser();
                       
                        var adminUserRole=new UserRole { Role = adminRole, User = adminUser };
                        var userUserRole=new UserRole { Role = userRole, User = adminUser };
                        var userRoles=new UserRole[]{adminUserRole,userUserRole};
                        adminUser.UserRoles=userRoles;
                        var claims= GetUserClaims(adminUser.Id);
                        adminUser.UserClaims=claims;                   
                        context.Add(adminUser);                       
                    }

                    if(!context.Browsers.Any())
                    {
                       
                        var browsers= CreateBrowsers();
                        context.AddRange(browsers);
                    }
                    context.SaveChanges();
                }
            }
        }

        private Policy CreatePolicy()
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
                        PasswordContainsUppercase=false,
                        CanUpdateDeviceId=true
                    };
            return policy;
        }
        private User CreateAdminUser()
        {
          var admin=  new User
                        {
                            Id=Guid.NewGuid(),
                            Username = "sysAdmin",
                            LowercaseUsername="sysadmin",
                            UserCode=256,
                            DisplayName = "مدیر سیستم",
                            IsActive = true,
                            LastLoggedIn = null,
                            Password = _securityService.GetSha256Hash("123456"),
                            SerialNumber = Guid.NewGuid().ToString("N"),
                            FirstName="مدیر" ,
                            LastName="سیستم" ,
                            JoinTimespan= DateTimeOffset.UtcNow,
                            Email="Sepehr@example.com",
                            LowercaseEmail="sepehr@example.com",
                            EmailConfirmed=false,
                            Mobile="09130000000",
                            MobileConfirmed=false,
                            IncludeThisRecord=true
                        };
                return admin;
        }
        private ICollection<Browser> CreateBrowsers()
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
                return browsers;
        }
        private string GetAuthLevelQuery()
        {
            var query=  @"INSERT [dbo].[AuthLevel1s] ([Id], [AppBoundaryCode], [AppBoundaryTitle]) VALUES (1, 1, N'مدیریت کاربران') 
            INSERT [dbo].[AuthLevel1s] ([Id], [AppBoundaryCode], [AppBoundaryTitle]) VALUES (2, 2, N'قرائت')        
            INSERT [dbo].[AuthLevel2s] ([Id], [AuthLevel1Id], [IconClass], [Title], [ElementId]) VALUES (1, 1, N'fa-lock', N'ایجاد کاربر', N'l2_1')        
            INSERT [dbo].[AuthLevel2s] ([Id], [AuthLevel1Id], [IconClass], [Title], [ElementId]) VALUES (2, 2, N'fa-info', N'اطلاعات', N'l2_2')        
            INSERT [dbo].[AuthLevel2s] ([Id], [AuthLevel1Id], [IconClass], [Title], [ElementId]) VALUES (3, 1, N'fa-info', N'کاربران', N'l2_3')        
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (1, 1, N'', N'Auth', N'', N'UserManager', N'Add', N'افزودن', N'l3_1')        
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (2, 2, N'', N'', N'', N'Programmer', N'RenderExecuteQueryAll', N'مشاهده', N'l3_2')        
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (3, 2, N'', N'', N'', N'RoleManger', N'M1', N'مباحثه', N'l3_3')        
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (4, 3, N'', N'', N'', N'UserManager', N'CreateUser', N'ثبت کاربر', N'l3_4')        
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (1, 1, N'همه موارد', N'UserManager.Add')        
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (2, 2, N'همه', N'Programmer.RenderExecuteQueryAll')        
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (3, 3, N'all', N'C2.M1')        
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (4, 3, N'all', N'C2.M2')        
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (5, 4, N'همه', N'UserManager.CreateUser')";
            return query;
        }
        private ICollection<UserClaim> GetUserClaims(Guid userId)
        {
             var claim=new UserClaim{ClaimType=CustomClaimTypes.Action,
                            ClaimValue="UserManager.Add",InsertBy=userId,IsActive=true,
                            InsertTimespan=DateTimeOffset.UtcNow};
            var claims=new []{claim};
            return claims;
        }
    }
}