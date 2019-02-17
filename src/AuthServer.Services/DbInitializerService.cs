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
                    var examinerRole =new Role { Name = CustomRoles.Examiner,TitleFa="ارزیاب",IsActive=true,NeedDeviceId=false };
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
            INSERT [dbo].[AuthLevel2s] ([Id], [AuthLevel1Id], [IconClass], [Title], [ElementId]) VALUES (6, 2, N'fa-search', N'جستجو وگردش دیتا', N'l2_6')            
            INSERT [dbo].[AuthLevel2s] ([Id], [AuthLevel1Id], [IconClass], [Title], [ElementId]) VALUES (7, 2, N'fa-file-o', N'دریافت وایجاد فایل', N'l2_7')            
            INSERT [dbo].[AuthLevel2s] ([Id], [AuthLevel1Id], [IconClass], [Title], [ElementId]) VALUES (8, 2, N'fa-archive', N'فایلهای ارسال نشده', N'l2_8')            
            INSERT [dbo].[AuthLevel2s] ([Id], [AuthLevel1Id], [IconClass], [Title], [ElementId]) VALUES (9, 2, N'fa-file-text', N'فایلهای بارگیری شده', N'l2_9')            
            INSERT [dbo].[AuthLevel2s] ([Id], [AuthLevel1Id], [IconClass], [Title], [ElementId]) VALUES (10, 2, N'fa-list-alt', N'فایلهای درحال قرائت', N'l2_10')
            INSERT [dbo].[AuthLevel2s] ([Id], [AuthLevel1Id], [IconClass], [Title], [ElementId]) VALUES (11, 2, N'fa-clipboard', N'فایلهای تخلیه شده', N'l2_11')
            INSERT [dbo].[AuthLevel2s] ([Id], [AuthLevel1Id], [IconClass], [Title], [ElementId]) VALUES (12, 2, N'fa-bar-chart', N'گزارشات', N'l2_12')
            INSERT [dbo].[AuthLevel2s] ([Id], [AuthLevel1Id], [IconClass], [Title], [ElementId]) VALUES (13, 2, N'fa-dashboard', N'مدیریت', N'l2_13')
            
            INSERT [dbo].[AuthLevel2s] ([Id], [AuthLevel1Id], [IconClass], [Title], [ElementId]) VALUES (14, 2, N'fa-mobile-phone', N'مدیریت APK', N'l2_14')
            
            INSERT [dbo].[AuthLevel2s] ([Id], [AuthLevel1Id], [IconClass], [Title], [ElementId]) VALUES (15, 2, N'fa-drupal', N'آب بها', N'l2_15')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (1, 1, N'', N'CRM', N'', N'UserManager', N'CreateUser', N'افزودن کاربر', N'l3_1')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (2, 1, N'', N'', N'', N'UserManager', N'EditUser', N'ویرایش کاربر', N'l3_2')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (3, 1, N'', N'CRM', N'', N'UserManager', N'Index', N'مشاهده کاربران', N'l3_3')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (4, 3, N'', N'CRM', N'', N'RoleManager', N'Index', N'مشاهده گروه ها', N'l3_4')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (5, 1, N'', N'CRM', N'', N'UserManager', N'SearchPro', N'جستجوی پیشرفته', N'l3_5')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (6, 4, N'', N'CRM', N'', N'PolicyManager', N'PasswordPolicy', N'تنظیمات کلمه عبور', N'l3_6')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (7, 5, N'', N'CRM', N'', N'Profile', N'Index', N'پروفایل', N'l3_7')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (8, 5, N'', N'Auth', N'', N'UserManager', N'ChangePassword', N'تغییر پسوورد', N'l3_8')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (9, 7, N'', N'CRM', N'', N'CRReceiveOrGenerateFile', N'ReceiveData', N'دریافت بصورت خودکار', N'l3_9')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (10, 7, N'', N'CRM', N'', N'DoFragment', N'Index', N'دریافت نوبتی', N'l3_10')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (11, 13, N'', N'CRM', N'', N'FileFragment', N'Index', N'مدیریت نوبتی ها', N'l3_11')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (12, 13, N'', N'CRM', N'', N'FileFragmentDetails', N'Index', N'مدیریت مسیرها', N'l3_12')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (13, 13, N'', N'CRM', N'', N'CounterState', N'Index', N'مدیریت کدهای قرائت', N'l3_13')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (14, 13, N'', N'CRM', N'', N'ReportValueKeyManagement', N'Index', N'مدیریت گزارشات بازرسی', N'l3_14')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (15, 13, N'', N'CRM', N'', N'DefaultValueManagement', N'Index', N'تنظیمات پیش فرض قرائت', N'l3_15')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (16, 13, N'', N'CRM', N'', N'TrackingControl', N'Index', N'مانیتور شماره پیگیری', N'l3_16')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (17, 13, N'', N'CRM', N'', N'CounterReadingMatchField', N'Index', N'انطباق فیلد ها', N'l3_17')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (18, 13, N'', N'CRM', N'', N'CounterReadingFileGroup', N'Index', N'گروه فایل های ورودی/خروجی', N'l3_18')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (19, 13, N'', N'CRM', N'', N'CounterReadingFilePart', N'Index', N'ساختار فایل های ورودی/خروجی', N'l3_19')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (20, 8, N'', N'CRM', N'', N'CRImportedFile', N'Index', N'مشاهده فایل های ارسال نشده', N'l3_20')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (21, 8, N'', N'', N'', N'ListHelper', N'DisplayListByListNum', N'مشاهده جزئیات لیست', N'l3_21')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (22, 15, N'', N'', N'', N'Qabs', N'DispalyLast', N'مشاهده آخرین قبض', N'l3_22')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (23, 15, N'', N'CRM', N'', N'OnAirBilling', N'DislayAirMasrafRateParams', N'محاسبه مصرف و متوسط  مصرف', N'l3_23')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (24, 15, N'', N'CRM', N'', N'OnAirBilling', N'DisplayDateDifference', N'محاسبه اختلاف تاریخ', N'l3_24')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (25, 15, N'', N'CRM', N'', N'OnAirBilling', N'DisplayQabsParams', N'محاسبه سریع قبض', N'l3_25')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (26, 9, N'', N'CRM', N'', N'CRLoadedFile', N'Index', N'مشاهده فایل های بارگیری شده', N'l3_26')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (27, 10, N'', N'CRM', N'', N'CRReadingNowFile', N'GetOveralReadingNow', N'فهرست فایلهای در حال قرائت', N'l3_27')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (28, 10, N'', N'', N'', N'EmergencyOnOffload', N'Offload', N'تخلیه اضطراری فایل', N'l3_28')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (29, 11, N'', N'CRM', N'', N'CRSendOrGenerateFile', N'Index', N'فایلهای تخلیه شده', N'l3_29')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (30, 11, N'', N'CRM', N'', N'CRSendOrGenerateFile', N'DisplayGeneratedFiles', N'فایلهای دانلود شده', N'l3_30')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (31, 11, N'', N'', N'', N'Bazdid', N'SetBazdid', N'ثبت بازدید', N'l3_31')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (32, 11, N'', N'', N'', N'Bazdid', N'GetPreBazdid', N'تعیین مامور بازدید', N'l3_32')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (33, 12, N'', N'CRM', N'', N'CRReportManager', N'Index', N'گزارشات بازرسی', N'l3_33')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (34, 12, N'', N'CRM', N'', N'CRReportManager', N'DisplayReportDetails', N'جزئیات گزارش بازرسی', N'l3_34')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (35, 12, N'', N'CRM', N'', N'CRQeireMojaz', N'Index', N'گزارش غیر مجاز', N'l3_35')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (36, 12, N'', N'CRM', N'', N'KarkerdMamoor', N'Index', N'گزارش کارکرد', N'l3_36')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (37, 12, N'', N'CRM', N'', N'Peimayesh', N'Index', N'گزارش پیمایش', N'l3_37')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (38, 12, N'', N'CRM', N'', N'GeoReport', N'Index', N'گزارش های مکانی', N'l3_38')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (39, 6, N'', N'CRM', N'', N'History', N'Index', N'جستجو', N'l3_39')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (40, 6, N'', N'CRM', N'', N'History', N'SearchTajmie', N'جستجوی تجمیعی', N'l3_40')
            
            INSERT [dbo].[AuthLevel3s] ([Id], [AuthLevel2Id], [Domain], [PreRoute], [Parameters], [Controller], [Action], [Title], [ElementId]) VALUES (41, 6, N'', N'CRM', N'', N'History', N'SearchInMoshtaraks', N'جستجو مشترک', N'l3_41')
            
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
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (14, 9, N'مشاهده جدول و ورود پارامتر ها', N'CRReceiveOrGenerateFile.ReceiveData')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (15, 9, N'خواندن اطلاعات جدول', N'CRReceiveOrGenerateFile.Read')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (16, 9, N'انتخاب مامور و بازنشانی اطلاعات', N'CRReceiveOrGenerateFile.MergeIntoLoadedData')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (17, 10, N'مشاهده جدول و  ورود پارامتر ها', N'DoFragment.Index')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (18, 10, N'خواندن اطلاعات جدول', N'DoFragment.Read')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (19, 11, N'مشاهده جدول', N'FileFragment.Index')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (20, 11, N'خواندن اطلاعات جدول', N'FileFragment.Read')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (21, 11, N'افزودن نوبتی', N'FileFragment.Create')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (22, 11, N'ویرایش نوبتی', N'FileFragment.Update')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (23, 11, N'حذف نوبتی', N'FileFragment.Remove')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (24, 11, N'تایید مسیر های ساخته شده', N'FileFragment.ValidateRoute')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (25, 12, N'مشاهده جدول و ورود پارامتر ها', N'FileFragmentDetails.Index')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (26, 12, N'خواندن اطلاعات جدول', N'FileFragmentDetails.Read')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (27, 12, N'ویرایش مسیر', N'FileFragmentDetails.Update')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (28, 12, N'حذف مسیر', N'FileFragmentDetails.Delete')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (29, 13, N'مشاهده جدول', N'CounterState.Index')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (30, 13, N'خواندن اطلاعات جدول', N'CounterState.Read')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (31, 13, N'ویرایش ', N'CounterState.Update')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (32, 13, N'مشاهده جدول', N'ReportValueKeyManagement.Index')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (33, 13, N'خواندن اطلاعات جدول', N'ReportValueKeyManagement.Read')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (34, 13, N'ویرایش ', N'ReportValueKeyManagement.Update')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (35, 16, N'مشاهده جدول ', N'TrackingControl.Index')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (36, 16, N'ورود شماره پیگیری و دریافت اطلاعات', N'TrackingControl.Read')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (37, 16, N'ویرایش ', N'TrackingControl.Update')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (38, 16, N'حذف', N'TrackingControl.Remove')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (39, 17, N'مشاهده جدول', N'CounterReadingMatchField.Index')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (40, 17, N'خواندن اطلاعات جدول', N'CounterReadingMatchField.Read')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (41, 17, N'ایجاد', N'CounterReadingMatchField.Create')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (42, 18, N'مشاهده جدول', N'CounterReadingFileGroup.Index')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (43, 18, N'خواندن اطلاعات جدول', N'CounterReadingFileGroup.Read')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (44, 18, N'ایجاد', N'CounterReadingFileGroup.Create')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (45, 18, N'ویرایش', N'CounterReadingFileGroup.Update')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (46, 19, N'مشاهده جدول', N'CounterReadingFilePart.Index')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (47, 19, N'خواندن اطلاعات جدول', N'CounterReadingFilePart.Read')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (48, 19, N'ایجاد', N'CounterReadingFilePart.Create')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (49, 19, N'ویرایش', N'CounterReadingFilePart.Update')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (50, 14, N'مشاهده جدول', N'ReportValueKeyManagement.Index')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (51, 14, N'خواندن اطلاعات جدول', N'ReportValueKeyManagement.Read')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (52, 14, N'ویرایش', N'ReportValueKeyManagement.Update')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (53, 20, N'مشاهده جدول', N'CRImportedFile.Index')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (54, 20, N'خواندن اطلاعات جدول', N'CRImportedFile.Read')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (55, 20, N'مشاهده مقادیر پیش فرض قرائت', N'CRImportedFile.DisplayDefaultValue')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (56, 20, N'تغییر مقادیر پیش فرض قرائت', N'CRImportedFile.OverrideDefaultValues')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (57, 20, N'حذف فایل ها', N'CRImportedFile.DeleteFile')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (58, 21, N'مشاهده فرم اصلی جزئیات لیست', N'ListHelper.DisplayListByListNum')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (59, 21, N'مشاهده جدول و اطلاعات آن', N'ListHelper.ReadListByListNum')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (60, 20, N'جابجایی مامور (انتقال فایل به مامور دیگر)', N'CRImportedFile.ReplaceMamoor')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (61, 22, N'مشاهده ', N'Qabs.DisplayLast')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (62, 23, N'مشاهده فرم و امکان ورود پارامتر ها', N'OnAirBilling.DislayAirMasrafRateParams')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (63, 23, N'امکان ارسال پارامتر ها به سرور و انجام محاسبه', N'OnAirBilling.CalculateAirMasrafRate')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (64, 24, N'مشاهده فرم و ورود پارامتر ها', N'OnAirBilling.DisplayDateDifference')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (65, 24, N'ارسال پارامتر ها و محاسبه', N'OnAirBilling.CalcDateDifference')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (66, 25, N'نمایش فرم و ورود پارامترها', N'OnAirBilling.DisplayQabsParams')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (67, 25, N'ارسال پارامترها و محاسبه', N'OnAirBilling.CalculateQuickQabs')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (68, 26, N'مشاهده جدول', N'CRLoadedFile.Index')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (69, 26, N'خواندن اطلاعات جدول', N'CRLoadedFile.Read')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (70, 26, N'برگشت فایل به بخش فایلهای ارسال نشده', N'CRLoadedFile. Back')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (71, 27, N'مشاهده جدول ', N'CRReadingNowFile.GetOveralReadingNow')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (72, 27, N'خواندن اطلاعات جدول', N'CRReadingNowFile.ReadOveralReadingNow')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (73, 27, N'مشاهده جدول اطلاعات توصیفی', N'CRReadingNowFile.GetReadingNowSingle')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (74, 27, N'خواندن اطلاعات جدول اطلاعات توصیفی', N'CRReadingNowFile.ReadReadingNowSingle')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (75, 27, N'ثبت بارگیری ویژه برای مامور', N'CRReadingNowFile.RegisterSpecialLoad')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (76, 28, N'مشاهده فرم تخلیه اضطراری', N'EmergencyOnOffload.Offload')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (77, 28, N'انجام تخلیه اضطراری', N'EmergencyOnOffload.DoOffload')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (78, 29, N'مشاهده جدول', N'CRSendOrGenerateFile.Index')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (79, 29, N'خواندن اطلاعات جدول', N'CRSendOrGenerateFile.Read')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (80, 29, N'ایجاد و دانلود فایل خروجی', N'CRSendOrGenerateFile.GetOrGenerateOffloadedFile')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (81, 30, N'مشاهده جدول', N'CRSendOrGenerateFile.DisplayGeneratedFiles')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (82, 30, N'خواندن اطلاعات جدول', N'CRSendOrGenerateFile.ReadGeneratedFiles')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (83, 30, N' حذف فایل تولید شده و بازگشت فایل به مرحله تخلیه شده', N'CRSendOrGenerateFile.DeleteGeneratedFileAnd Back')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (84, 31, N'مشاهده جدول ثبت بازدید و امکان ورود پارامترها', N'Bazdid.SetBazdid')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (85, 31, N'ارسال پارامتر ها و انجام ثبت  یا حذف بازدید', N'Bazdid.DoSetBazdid')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (86, 31, N'مشاهده جزئیات بازدید های ثبت یا حذف شده', N'Bazdid.ReadBazdidDetails')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (87, 31, N'ثبت یا حذف بازدید بصورت موردی', N'Bazdid.UpdateSingleBazdid')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (88, 32, N'مشاهده فرم انتخاب شماره لیست و ناحیه جهت تخصیص شماره پیگیری', N'Bazdid.GetPreBazdid')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (89, 32, N'مشاهده جدول مربوط به اختصاص مامور بازدید', N'Bazdid.BreakBazdid')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (90, 32, N'خواندن اطلاعات از جدول اختصاص مامور بازدید', N'Bazdid.ReadBreakBazdid')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (91, 32, N'به روز رسانی اطلاعات اختصاص مامور بازدید', N'Bazdid.UpdateBreakBazdid')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (92, 32, N'افزودن مامور بازدید و ثبت آن', N'Bazdid.AddBreakBazdid')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (93, 32, N'حذف مامور بازدید و اطلاعات آن', N'Bazdid.RemoveBreakBazdid')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (94, 32, N'تایید موارد اختصاص یافته و دریافت شماره پیگیری', N'Bazdid.ConfirmBreakBazdid')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (95, 33, N'مشاهده جدول گزارش یک', N'CRReportManager.Index')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (96, 33, N'خواندن اطلاعات جدول ', N'CRReportManager.Read')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (97, 34, N'مشاهده جدول ', N'CRReportManager.DisplayReportDetails')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (98, 34, N'خواندن اطلاعات جدول', N'CRReportManager.ReadReportDetails')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (99, 35, N'مشاهده جدول ', N'CRQeireMojaz.Index')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (100, 35, N'خواندن اطلاعات جدول', N'CRQeireMojaz.Read')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (101, 36, N'مشاهده جدول', N'KarkerdMamoor.Index')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (102, 36, N'خواندن اطلاعات جدول', N'KarkerdMamoor.Read')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (103, 37, N'مشاهده جدول', N'Peimayesh.Index')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (104, 37, N'خواندن اطلاعات جدول', N'Peimayesh.Read')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (105, 38, N'مشاهده فرم اصلی و ورود پارامترها', N'GeoReport.Index')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (106, 38, N'مشاهده اطلاعات جغرافیای شماره پیگیری(اطلاعات مکانی)', N'GeoReport.GetGeolocationInfo')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (107, 38, N'نمایش اطلاعات مسیر شماره پیگیری بر اساس روز', N'GeoReport.GetRouteInfo')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (108, 38, N'نمایش نقاط  و خوشه بندی بر اساس عنوان گزارش', N'GeoReport.GetReportPointInfos')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (109, 32, N'دریافت فهرست شماره لیست های نیازمند تعیین مامور بازدید بر اساس منطقه انتخاب شده', N'Bazdid.GetBazdidMasters')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (110, 10, N'مشاهده فرم تقسیم فایل و انتخاب مامور', N'DoFragment.GetFragmentDetail')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (111, 10, N'خواندن اطلاعات تقسیم فایل', N'DoFragment.ReadFragmentDetails')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (112, 10, N'ذخیره تغییرات تقسیم فایل', N'DoFragment.SaveChanges')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (113, 39, N'مشاهده فرم اصلی و ورود پارامتر ها', N'History.Index')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (114, 39, N'خواندن اطلاعات جدول اصلی', N'History.ReadMaster')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (115, 39, N'ادامه و مشاهده جزئیات گردش فایل قرائت', N'History.SearchByTrackNumber')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (116, 39, N'مشاهده اطلاعات توصیفی هر لیست', N'History.ReadDetails')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (117, 40, N'مشاهده جدول  و ورود پارامتر های جستجو', N'History.SearchTajmie')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (118, 40, N'خواندن اطلاعات جدول', N'History.ReadSearchTajmie')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (119, 41, N'مشاهده فرم اصلی ، جدول و انتخاب آیتم که جستجو بر آن اساس انجام شود', N'History.SearchInMoshtaraks')
            
            INSERT [dbo].[AuthLevel4s] ([Id], [AuthLevel3Id], [Title], [Value]) VALUES (120, 41, N'خواندن اطلاعات ', N'History.ReadSearchInMoshtaraks')";
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
            var editUserClaim=new UserClaim{ClaimType=CustomClaimTypes.Action,
                ClaimValue="UserManager.EditUser",InsertBy=userId,IsActive=true,InsertTimespan=DateTimeOffset.UtcNow};
            var claims=new []{addClaim,allUserClaim,allUserReadClaim,editUserClaim};
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