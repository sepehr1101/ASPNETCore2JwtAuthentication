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
                    var adminRole = new Role { Name = CustomRoles.Admin,TitleFa="مدیر سیستم",IsActive=true,NeedDeviceId=false };
                    var userRole = new Role { Name = CustomRoles.User,TitleFa="کاربر",IsActive=true,NeedDeviceId=false };
                    var counterReadingRole = new Role { Name = CustomRoles.CounterReader,TitleFa="مامور قرائت",IsActive=true,NeedDeviceId=true };
                    if (!context.Roles.Any())
                    {                       
                        context.Add(adminRole);
                        context.Add(userRole);
                        context.Add(counterReadingRole);
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
                       
                        var adminUserRole=new UserRole { Role = adminRole, User = adminUser,IsActive=true };
                        var userUserRole=new UserRole { Role = userRole, User = adminUser,IsActive=true };
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
            var query=  @"
            INSERT [dbo].[AuthLevel1s] ([Id], [AppBoundaryCode], [AppBoundaryTitle], [InSidebar]) VALUES (1, 1, N'مدیریت سیستم', 1)
            INSERT [dbo].[AuthLevel1s] ([Id], [AppBoundaryCode], [AppBoundaryTitle], [InSidebar]) VALUES (2, 2, N'مدیریت سامانه قرائت', 1)
            INSERT [dbo].[AuthLevel1s] ([Id], [AppBoundaryCode], [AppBoundaryTitle], [InSidebar]) VALUES (3, 3, N'تنظیمات شخصی', 0)
            INSERT [dbo].[AuthLevel2s] ([Id], [AuthLevel1Id], [IconClass], [Title], [ElementId]) VALUES (1, 1, N'fa-user', N'مدیریت کاربران', N'l2_1')
            INSERT [dbo].[AuthLevel2s] ([Id], [AuthLevel1Id], [IconClass], [Title], [ElementId]) VALUES (2, 2, N'fa-info', N'اطلاعات', N'l2_2')
            INSERT [dbo].[AuthLevel2s] ([Id], [AuthLevel1Id], [IconClass], [Title], [ElementId]) VALUES (3, 1, N'fa-group', N'مدیریت گروه  ها', N'l2_3')
            INSERT [dbo].[AuthLevel2s] ([Id], [AuthLevel1Id], [IconClass], [Title], [ElementId]) VALUES (4, 1, N'fa-lock', N'تنظیمات سیستم', N'l2_4')
            INSERT [dbo].[AuthLevel2s] ([Id], [AuthLevel1Id], [IconClass], [Title], [ElementId]) VALUES (5, 3, N'', N'تنظیمات شخصی', N'l2_5')
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (1, 1, N'', N'', N'', N'UserManager', N'CreateUser', N'افزودن کاربر', N'l3_1')
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (2, 1, N'', N'', N'', N'UserManager', N'EditUser', N'ویرایش کاربر', N'l3_2')
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (3, 1, N'', N'', N'', N'UserManager', N'Index', N'مشاهده کاربران', N'l3_3')
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (4, 3, N'', N'', N'', N'RoleManager', N'Index', N'مشاهده گروه ها', N'l3_4')
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (5, 1, N'', N'', N'', N'UserManager', N'SearchPro', N'جستجوی پیشرفته', N'l3_5')
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (6, 4, N'', N'', N'', N'PolicyManager', N'PasswordPolicy', N'تنظیمات کلمه عبور', N'l3_6')
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (7, 5, N'', N'', N'', N'Profile', N'Index', N'پروفایل', N'l3_7')
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (8, 5, N'', N'Auth', N'', N'UserManager', N'ChangePassword', N'تغییر پسوورد', N'l3_8')
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (1, 1, N'همه موارد', N'UserManager.CreateUser')
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (2, 3, N'مشاهده جدول همه کاربران', N'UserManager.Index')
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (3, 3, N'خواندن اطلاعات جدول همه کاربران', N'UserManager.ReadAllUsers')
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (4, 3, N'آزمایش', N'UserManager.Test')
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (5, 4, N'مشاهده جدول ', N'RoleManager.Index')
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (6, 4, N'خواندن اطلاعات جدول', N'RoleManager.ReadAll')
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (7, 4, N'ویرایش گروه', N'RoleManager.UpdateRole')
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (8, 5, N'انجام جستجو', N'UserManager.SearchPro')
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (9, 6, N'مشاهده', N'PolicyManager.PasswordPolicy')
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (10, 7, N'مشاهده', N'Profile.Index')
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (11, 3, N'مشاهده اطلاعات کاربر جهت ویرایش', N'UserManager.EditUser')
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (12, 3, N'مشاهده جدول اطلاعات ورود ها', N'LoginManager.UserLogins')
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (13, 3, N'خواندن اطلاعات جدول ورود ها', N'LoginManager.ReadUserLogins')
            ";
            return query;
        }
        private ICollection<UserClaim> GetUserClaims(Guid userId)
        {
             var addClaim=new UserClaim{ClaimType=CustomClaimTypes.Action,
                            ClaimValue="UserManager.Add",InsertBy=userId,IsActive=true,
                            InsertTimespan=DateTimeOffset.UtcNow};
            var allUserClaim=new UserClaim{ClaimType=CustomClaimTypes.Action,
                            ClaimValue="UserManager.Index",InsertBy=userId,IsActive=true,
                            InsertTimespan=DateTimeOffset.UtcNow};
            var allUserReadClaim=new UserClaim{ClaimType=CustomClaimTypes.Action,
                ClaimValue="UserManager.ReadAllUsers",InsertBy=userId,IsActive=true,
                InsertTimespan=DateTimeOffset.UtcNow};
            var claims=new []{addClaim,allUserClaim,allUserReadClaim};
            return claims;
        }

        private ICollection<OS> GetOsList()
        {
            var osList=new List<OS>()
            {
                new OS{AcceptRequestFrom=false,IconClass=String.Empty,Id=1,Title="Windows (iTunes)"},
                new OS{AcceptRequestFrom=false,IconClass=String.Empty,Id=2,Title="Windows RT"},
                new OS{AcceptRequestFrom=false,IconClass=String.Empty,Id=3,Title="Windows Phone"},
                new OS{AcceptRequestFrom=false,IconClass=String.Empty,Id=4,Title="Windows"},
                new OS{AcceptRequestFrom=false,IconClass=String.Empty,Id=5,Title=""},
                new OS{AcceptRequestFrom=false,IconClass=String.Empty,Id=6,Title=""},
                new OS{AcceptRequestFrom=false,IconClass=String.Empty,Id=7,Title=""},
                new OS{AcceptRequestFrom=false,IconClass=String.Empty,Id=8,Title=""},
                new OS{AcceptRequestFrom=false,IconClass=String.Empty,Id=9,Title=""},
                new OS{AcceptRequestFrom=false,IconClass=String.Empty,Id=10,Title=""},
                new OS{AcceptRequestFrom=false,IconClass=String.Empty,Id=11,Title=""},
                new OS{AcceptRequestFrom=false,IconClass=String.Empty,Id=12,Title=""},
                new OS{AcceptRequestFrom=false,IconClass=String.Empty,Id=13,Title=""},
                new OS{AcceptRequestFrom=false,IconClass=String.Empty,Id=14,Title=""},
                new OS{AcceptRequestFrom=false,IconClass=String.Empty,Id=15,Title=""},
                new OS{AcceptRequestFrom=false,IconClass=String.Empty,Id=16,Title=""},
            };
            return osList;
        }
    }
}