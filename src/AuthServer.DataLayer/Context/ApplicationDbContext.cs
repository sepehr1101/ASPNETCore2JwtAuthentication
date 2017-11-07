using AuthServer.DomainClasses;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;

using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;


namespace AuthServer.DataLayer.Context
{
    public class ApplicationDbContext : DbContext, IUnitOfWork
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        { 
            
        }

        public virtual DbSet<User> Users { set; get; }
        public virtual DbSet<Role> Roles { set; get; }
        public virtual DbSet<UserRole> UserRoles { get; set; }
        public virtual DbSet<UserToken> UserTokens { get; set; }
        public virtual DbSet<UserClaim> UserClaims{get;set;}
        public virtual DbSet<AuthLevel1> AuthLevel1s{get;set;}
        public virtual DbSet<AuthLevel2> AuthLevel2s{get;set;}
        public virtual DbSet<AuthLevel3> AuthLevel3s{get;set;}
        public virtual DbSet<AuthLevel4> AuthLevel4s{get;set;}
        public virtual DbSet<Policy> Policies{get;set;}
        public virtual DbSet<Login> Logins{get;set;}
        public virtual DbSet<Browser> Browsers { get; set; }
        public virtual DbSet<OS> OSes{get;set;}

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // it should be placed here, otherwise it will rewrite the following settings!
            base.OnModelCreating(builder);

            // Custom application mappings
            BuildUserModel(builder);
            BuildRoleModel(builder);
            BuildUserRoleModel(builder);    
            BuildUserTokenModel(builder);
            BuildUserClaimModel(builder);
            BuildAuthLevel1Model(builder);
            BuildAuthLevel2Model(builder);
            BuildAuthLevel3Model(builder);
            BuildAuthLevel4Model(builder);
            BuildPolicyModel(builder);
            BuildBrowserModel(builder);
            BuildOsModel(builder);
            BuildLoginModel(builder);
        }

        public DatabaseFacade GetDatabase()
        {
            return this.Database;
        }
     
        public List<T> ExecSQL<T>(string query)
        {
            var database=GetDatabase();            
            using (var command = database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;
                database.OpenConnection();

                using (var result = command.ExecuteReader())
                {
                    List<T> list = new List<T>();
                    T obj = default(T);
                    while (result.Read())
                    {
                        obj = Activator.CreateInstance<T>();
                        foreach (PropertyInfo prop in obj.GetType().GetProperties())
                        {
                            if (!object.Equals(result[prop.Name], DBNull.Value))
                            {
                                prop.SetValue(obj, result[prop.Name], null);
                            }
                        }
                        list.Add(obj);
                    }
                    return list;                
                }
            }            
        }
        
        #region Build Entities (13 Methods)        
        private void BuildUserModel(ModelBuilder builder)
        {
            builder.Entity<User>(entity =>
            {
                entity.Property(e => e.Username).HasMaxLength(450).IsRequired();              
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.UserCode).IsRequired();
                entity.HasIndex(e => e.UserCode).IsUnique();
                entity.Property(e => e.FirstName).HasMaxLength(450).IsRequired().IsUnicode();
                entity.Property(e => e.LastName).HasMaxLength(450).IsRequired().IsUnicode();
                entity.Property(e => e.Email).HasMaxLength(450).IsRequired();
                entity.Property(e => e.EmailConfirmed).IsRequired();
                entity.Property(e => e.Mobile).HasMaxLength(11).IsRequired();
                entity.Property(e => e.MobileConfirmed).IsRequired(true);
                entity.Property(e => e.JoinTimespan).IsRequired();                
                entity.Property(e => e.Password).IsRequired();
                entity.Property(e => e.SerialNumber).HasMaxLength(450);
                entity.HasOne(e => e.UserToken)
                      .WithOne(ut => ut.User)
                      .HasForeignKey<UserToken>(ut => ut.UserId); // one-to-one association
                
                entity.Property(e=>e.InvalidLoginAttemptCount).IsRequired();
                entity.Property(e=>e.IsLocked).IsRequired();
                entity.Property(e=>e.LockTimespan).IsRequired();
                entity.Property(e=>e.RequireRecaptcha).IsRequired();
                entity.Property(e=>e.IncludeThisRecord).IsRequired();
                entity.Property(e => e.DeviceId).IsRequired();
            });
        }
        private void BuildRoleModel(ModelBuilder builder)
        {
              builder.Entity<Role>(entity =>
            {               
                entity.Property(e => e.Name).HasMaxLength(450).IsRequired();
                entity.Property(e =>e.TitleFa).HasMaxLength(450).IsRequired();
                entity.Property(e=>e.IsActive).IsRequired();
                entity.Property(e => e.NeedDeviceId).IsRequired();
                entity.HasIndex(e => e.Name).IsUnique();
            });
        }
        private void BuildUserRoleModel(ModelBuilder builder)
        {
               builder.Entity<UserRole>(entity =>
            {
                entity.Property(e => e.Id).IsRequired();
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.RoleId);
                entity.Property(e => e.UserId);
                entity.Property(e => e.RoleId);
                entity.Property(e => e.IsActive);
                entity.HasOne(d => d.Role).WithMany(p => p.UserRoles).HasForeignKey(d => d.RoleId);
                entity.HasOne(d => d.User).WithMany(p => p.UserRoles).HasForeignKey(d => d.UserId);
            });
        }
        private void BuildUserTokenModel(ModelBuilder builder)
        {
              builder.Entity<UserToken>(entity =>
            {
            });
        }
        private void BuildUserClaimModel(ModelBuilder builder)
        {
             builder.Entity<UserClaim>(entity=>{
                entity.HasIndex(e=>e.UserId);
                entity.HasOne(e=>e.User).WithMany(e=>e.UserClaims).HasForeignKey(e=>e.UserId);
                entity.Property(e => e.IsActive).IsRequired();
                entity.Property(e=>e.InsertBy).IsRequired();
                entity.Property(e=>e.InsertTimespan).IsRequired();
            });
        }
        private void BuildAuthLevel1Model(ModelBuilder builder)
        {
               builder.Entity<AuthLevel1>(entity=>{
                entity.Property(e=>e.AppBoundaryTitle).HasMaxLength(255).IsRequired();
                entity.Property(e=>e.AppBoundaryCode).IsRequired();
                entity.Property(e => e.InSidebar).IsRequired();
            });
        }
        private void BuildAuthLevel2Model(ModelBuilder builder)
        {
              builder.Entity<AuthLevel2>(entity=>{
                entity.Property(e=>e.ElementId).HasMaxLength(255).IsRequired();
                entity.Property(e=>e.IconClass).IsRequired();
                entity.Property(e=>e.Title).HasMaxLength(255).IsRequired();
                entity.HasOne(e=>e.Parent).WithMany(e=>e.Children).HasForeignKey(e=>e.AuthLeve1Id);
            });
        }
        private void BuildAuthLevel3Model(ModelBuilder builder)
        {
              builder.Entity<AuthLevel3>(entity=>{
                entity.Property(e=>e.ElementId).HasMaxLength(255).IsRequired();
                entity.Property(e=>e.Action).HasMaxLength(255).IsRequired();
                entity.Property(e=>e.Controller).HasMaxLength(255).IsRequired();
                entity.Property(e=>e.Domain).HasMaxLength(255).IsRequired();
                entity.Property(e=>e.Parameters).HasMaxLength(255).IsRequired();
                entity.Property(e=>e.PreRoute).HasMaxLength(255).IsRequired();
                entity.Property(e=>e.Title).HasMaxLength(255).IsRequired();
                entity.HasOne(e=>e.Parent).WithMany(e=>e.Children).HasForeignKey(e=>e.AuthLevel2Id);
            });
        }
        private void BuildAuthLevel4Model(ModelBuilder builder)
        {
              builder.Entity<AuthLevel4>(entity=>{
                entity.Property(e=>e.Title).HasMaxLength(255).IsRequired();
                entity.Property(e=>e.Value).IsRequired();
                entity.HasOne(e=>e.Parent).WithMany(e=>e.Children).HasForeignKey(e=>e.AuthLevel3Id);
            });
        }
        private void BuildPolicyModel(ModelBuilder builder)
        {
            builder.Entity<Policy>(entity =>{
                entity.Property(e=>e.EnableValidIpRecaptcha).IsRequired();
                entity.Property(e=>e.IsActive).IsRequired();
                entity.Property(e=>e.LockInvalidAttempts).IsRequired();
                entity.Property(e => e.LockMin).IsRequired();
                entity.Property(e=>e.RequireRecaptchaInvalidAttempts).IsRequired();
                entity.Property(e=>e.MinPasswordLength).IsRequired();
                entity.Property(e=>e.PasswordContainsLowercase).IsRequired();
                entity.Property(e=>e.PasswordContainsNonAlphaNumeric).IsRequired();
                entity.Property(e=>e.PasswordContainsNumber).IsRequired();
                entity.Property(e=>e.PasswordContainsUppercase).IsRequired();
                entity.Property(e => e.CanUpdateDeviceId).IsRequired().HasDefaultValue(false);
            });
        }
        private void BuildBrowserModel(ModelBuilder builder)
        {
            builder.Entity<Browser>(entity =>{
                entity.Property(e=>e.TitleEng).HasMaxLength(31).IsRequired();
                entity.Property(e=>e.TitleFa).HasMaxLength(31).IsRequired();              
                entity.Property(e=>e.IconClass).HasMaxLength(31).IsRequired();    
                entity.Property(e=>e.AcceptRequestFrom).IsRequired();    
            });
        }
        private void BuildOsModel(ModelBuilder builder)
        {
             builder.Entity<OS>(entity =>{
                entity.Property(e=>e.Title).HasMaxLength(31).IsRequired();
                entity.Property(e=>e.AcceptRequestFrom).IsRequired();              
                entity.Property(e=>e.IconClass).HasMaxLength(31).IsRequired();    
            });
        }
        private void BuildLoginModel(ModelBuilder builder)
        {
              builder.Entity<Login>(entity =>{
                entity.Property(e=>e.LoginIP).HasMaxLength(63);
                entity.Property(e=>e.LoginTimespan).IsRequired();
                entity.Property(e=>e.WasSuccessful).IsRequired();
                entity.HasOne(e=>e.Browser).WithMany(e=>e.Logins).HasForeignKey(e=>e.BrowserId);
                entity.HasOne(e=>e.User).WithMany(e=>e.Logins).HasForeignKey(e=>e.UserId);
                entity.HasOne(e =>e.OS).WithMany(e=>e.Logins).HasForeignKey(e=>e.OsId);
                entity.Property(e =>e.BrowserVersion).HasMaxLength(31);
                entity.Property(e=>e.OsVersion).HasMaxLength(31);
                entity.Property(e =>e.OsTitle).HasMaxLength(31);
                entity.Property(e =>e.BrowserTitle).HasMaxLength(31);
            });
        }

        #endregion
    }
}