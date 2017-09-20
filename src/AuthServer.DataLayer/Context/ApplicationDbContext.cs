using AuthServer.DomainClasses;
using Microsoft.EntityFrameworkCore;

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
        public virtual DbSet<AuthLeve1> AuthLevel1s{get;set;}
        public virtual DbSet<AuthLeve2> AuthLevel2s{get;set;}
        public virtual DbSet<AuthLeve3> AuthLevel3s{get;set;}
        public virtual DbSet<AuthLeve4> AuthLevel4s{get;set;}

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // it should be placed here, otherwise it will rewrite the following settings!
            base.OnModelCreating(builder);

            // Custom application mappings
            builder.Entity<User>(entity =>
            {
                entity.Property(e => e.Username).HasMaxLength(450).IsRequired();
                entity.HasIndex(e => e.Username).IsUnique();
                
                entity.Property(e => e.Password).IsRequired();
                entity.Property(e => e.SerialNumber).HasMaxLength(450);
                entity.HasOne(e => e.UserToken)
                      .WithOne(ut => ut.User)
                      .HasForeignKey<UserToken>(ut => ut.UserId); // one-to-one association
            });

            builder.Entity<Role>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(450).IsRequired();
                entity.HasIndex(e => e.Name).IsUnique();
            });

            builder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.RoleId);
                entity.Property(e => e.UserId);
                entity.Property(e => e.RoleId);
                entity.HasOne(d => d.Role).WithMany(p => p.UserRoles).HasForeignKey(d => d.RoleId);
                entity.HasOne(d => d.User).WithMany(p => p.UserRoles).HasForeignKey(d => d.UserId);
            });

            builder.Entity<UserToken>(entity =>
            {
            });

            builder.Entity<UserClaim>(entity=>{
                entity.HasIndex(e=>e.UserId);
                entity.HasOne(e=>e.User).WithMany(e=>e.UserClaims).HasForeignKey(e=>e.UserId);
            });

            builder.Entity<AuthLeve1>(entity=>{
                entity.Property(e=>e.AppBoundaryTitle).HasMaxLength(255).IsRequired();
                entity.Property(e=>e.AppBoundaryCode).IsRequired();
            });

            builder.Entity<AuthLeve2>(entity=>{
                entity.Property(e=>e.ElementId).HasMaxLength(255).IsRequired();
                entity.Property(e=>e.IconClass).IsRequired();
                entity.Property(e=>e.Title).HasMaxLength(255).IsRequired();
                entity.HasOne(e=>e.Parent).WithMany(e=>e.Children).HasForeignKey(e=>e.AuthLeve1Id);
            });

             builder.Entity<AuthLeve3>(entity=>{
                entity.Property(e=>e.ElementId).HasMaxLength(255).IsRequired();
                entity.Property(e=>e.Action).HasMaxLength(255).IsRequired();
                entity.Property(e=>e.Controller).HasMaxLength(255).IsRequired();
                entity.Property(e=>e.Domain).HasMaxLength(255).IsRequired();
                entity.Property(e=>e.Parameters).HasMaxLength(255).IsRequired();
                entity.Property(e=>e.PreRoute).HasMaxLength(255).IsRequired();
                entity.Property(e=>e.Title).HasMaxLength(255).IsRequired();
                entity.HasOne(e=>e.Parent).WithMany(e=>e.Children).HasForeignKey(e=>e.AuthLeve2Id);
            });

              builder.Entity<AuthLeve4>(entity=>{
                entity.Property(e=>e.Title).HasMaxLength(255).IsRequired();
                entity.Property(e=>e.Value).IsRequired();
                entity.HasOne(e=>e.Parent).WithMany(e=>e.Children).HasForeignKey(e=>e.AuthLeve3Id);
            });
        }
    }
}