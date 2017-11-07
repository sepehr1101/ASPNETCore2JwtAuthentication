using System;
using System.Linq;
using System.Collections.Generic;
using AuthServer.Common;
using AuthServer.DataLayer.Context;
using AuthServer.DomainClasses;
using AuthServer.DomainClasses.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Security.Claims;

namespace AuthServer.Services
{
    public interface IRolesService
    {
        /// <summary>
        /// Get All Roles
        /// </summary>
        /// <returns>ICollection<Role></returns>
        Task<ICollection<Role>> GetAsync();
        Task<Role> GetAsync(int roleId);
        Task<List<Claim>> GetRolesAsClaimsAsync(Guid userId);
        Task<bool> IsUserInRole(Guid userId, string roleName);
        Task<List<User>> FindUsersInRoleAsync(string roleName);
        Task<List<User>> FindUsersInRoleAsync(int roleId);
        Task AddAsync(Role role);
        Task AddRangeAsync(ICollection<UserRole> userRoles);
        ICollection<UserRole> ConvertToUserRoles(ICollection<int>roleIds);
        ICollection<UserRole> ConvertToUserRoles(ICollection<int>roleIds,Guid userId);
        Task DisablePreviuosRoles(Guid userId);
        Task<ICollection<RoleInfo>> GetUserRoleInfoAsync(Guid userId);
        Task UpdateRole(Role role);
        IQueryable<UserRole> AddRolesToQuery(ICollection<int> roleIds);
        Task<bool> IsDeviceIdRequired(Guid userId);
    }

    public class RolesService : IRolesService
    {
        private readonly IUnitOfWork _uow;
        private readonly DbSet<Role> _roles;
        private readonly DbSet<User> _users;
        private readonly DbSet<UserRole> _userRoles;
        private IQueryable<UserRole> _userRoleQuery;

        public RolesService(IUnitOfWork uow)
        {
            _uow = uow;
            _uow.CheckArgumentIsNull(nameof(_uow));

            _roles = _uow.Set<Role>();
            _users = _uow.Set<User>();
            _userRoles = _uow.Set<UserRole>();

            _userRoleQuery=_userRoles;
        }

        public Task<List<Claim>> GetRolesAsClaimsAsync(System.Guid userId)
        {
            var userRolesQuery = from role in _roles.Where(r => r.IsActive)
                                 from userRoles in role.UserRoles.Where(r => r.IsActive)
                                 where userRoles.UserId == userId
                                 select new Claim(ClaimTypes.Role,role.Name);
            var roleAsClaims= userRolesQuery.OrderBy(x => x.Value).ToListAsync();
            return roleAsClaims;
        }

        public async Task<bool> IsUserInRole(System.Guid userId, string roleName)
        {
            var userRolesQuery = from role in _roles
                                 where role.Name == roleName
                                 from user in role.UserRoles
                                 where user.UserId == userId 
                                 select role;
            var userRole = await userRolesQuery.FirstOrDefaultAsync().ConfigureAwait(false);
            return userRole != null;
        }

        public Task<List<User>> FindUsersInRoleAsync(string roleName)
        {
            var roleUserIdsQuery = from role in _roles
                                   where role.Name == roleName && role.IsActive
                                   from user in role.UserRoles
                                   select user.UserId;
            return _users.Where(user => roleUserIdsQuery.Contains(user.Id) && user.IncludeThisRecord)
                         .ToListAsync();
        }
         public Task<List<User>> FindUsersInRoleAsync(int roleId)
        {
            var roleUserIdsQuery = from role in _roles
                                   where role.Id == roleId && role.IsActive
                                   from user in role.UserRoles
                                   select user.UserId;
            return _users.Where(user => roleUserIdsQuery.Contains(user.Id) && user.IncludeThisRecord)
                         .ToListAsync();
        }

        public async Task<Role> GetAsync(int roleId)
        {
            var role=await _roles.FirstOrDefaultAsync(r => r.IsActive && r.Id==roleId)
                .ConfigureAwait(false);
            return  role;
        }
        public async Task<ICollection<Role>> GetAsync()
        {
            var roles=await _roles.Where(r => r.IsActive)
                .ToListAsync().ConfigureAwait(false);
            return roles;
        }

        public async Task AddAsync(Role role)
        {  
            await _roles.AddAsync(role).ConfigureAwait(false);
        }
        public async Task AddRangeAsync(ICollection<UserRole> userRoles)
        {
            await _userRoles.AddRangeAsync(userRoles);
        }
        public ICollection<UserRole> ConvertToUserRoles(ICollection<int>roleIds)
        {
            var userRoles=new List<UserRole>();
            if(roleIds==null || roleIds.Count<1)
            {
               return null;
            }
            foreach(var roleId in roleIds)
            {
                var userRole=new UserRole();
                userRole.RoleId=roleId;
                userRoles.Add(userRole);
            }
            return userRoles;            
        }
        public ICollection<UserRole> ConvertToUserRoles(ICollection<int>roleIds,Guid userId)
        {
            var userRoles=new List<UserRole>();
            if(roleIds==null || roleIds.Count<1)
            {
               return null;
            }
            foreach(var roleId in roleIds)
            {
                var userRole=new UserRole();
                userRole.RoleId=roleId;
                userRole.UserId=userId;
                userRole.IsActive=true;
                userRoles.Add(userRole);
            }
            return userRoles;            
        }

        public async Task DisablePreviuosRoles(Guid userId)
        {
            var userRoles=await _userRoles.Where(u => u.UserId==userId)
                .ToListAsync();
            foreach(var userRole in userRoles)
            {
                userRole.IsActive=false;
            }
        }

        public async Task<ICollection<RoleInfo>> GetUserRoleInfoAsync(Guid userId)
        {
            var query=from role in _roles 
                from  userRole in _userRoles.Where(u => u.IsActive)
                    .Where(r => r.UserId==userId && r.RoleId==role.Id && r.IsActive).DefaultIfEmpty()
                select new RoleInfo
                {
                    IsSelected=userRole!=null,
                    Id=role.Id,
                    TitleEng=role.Name,
                    TitleFa=role.TitleFa
                };
            var roleInfos = await query.ToListAsync();
            return roleInfos;
        }

        public async Task UpdateRole(Role role)
        {
            var roleInDb=await _roles.FindAsync(role.Id).ConfigureAwait(false);
            roleInDb.IsActive=role.IsActive;
            roleInDb.TitleFa=role.TitleFa;
        }

        public IQueryable<UserRole> AddRolesToQuery(ICollection<int> roleIds)
        {
            if(roleIds==null || roleIds.Count<1)
            {
                return _userRoleQuery;
            }
            _userRoleQuery=_userRoleQuery.Where(u => roleIds.Contains(u.RoleId) && u.IsActive);
            return _userRoleQuery;
        }

        public async Task<bool> IsDeviceIdRequired(Guid userId)
        {
            var userNeedDeviceId=await _userRoles.Where(u => u.UserId==userId && u.IsActive)
                .Select(u => u.Role).Where(r => r.NeedDeviceId).AnyAsync();
            return userNeedDeviceId;
            
        }
    }
}