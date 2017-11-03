using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthServer.Common;
using AuthServer.DataLayer.Context;
using AuthServer.DomainClasses;
using Microsoft.EntityFrameworkCore;
using AuthServer.DomainClasses.ViewModels;

namespace AuthServer.Services
{
    public interface IUsersService
    {
        Task<string> GetSerialNumberAsync(Guid userId);
        Task<string> ChangeSerialNuberAsync(Guid userId);
        Task<User> FindUserAsync(string username, string password);
        Task<User> FindUserAsync(Guid userId);
        Task<User> FindUserAsync(string username);
        Task<ICollection<User>> Get(int take,int skip);
        Task UpdateUserLastActivityDateAsync(Guid userId);
        Task RegisterUserAsync(User user);
        Task UpdateDeviceSerialAsync(Guid userId,string deviceId);
        void UpdateUserAsync(User userInDb,UpdateUserViewModel userViewModel);
        Task<bool> CanFindUserAsync(int userCode);
        Task<bool> CanFindUserAsync(string lowercaseUsername);
        Task<bool> CanFindUserAsync(string lowercaseEmail,bool isEmail);
        Task<ICollection<User>> FindUsersAsync(IQueryable<UserClaim> userClaims,IQueryable<UserRole> userRoles);
        void ChangeMyPassword(User user,string newPassword);
        void FailedLoginAttempt(User user , Policy activePolicy);
         void SuccessLoginAttempt(User user);
        bool CanILogin(User user);
    }

    public class UsersService : IUsersService
    {
        private readonly IUnitOfWork _uow;
        private readonly DbSet<User> _users;
        private readonly ISecurityService _securityService;

        public UsersService(IUnitOfWork uow, ISecurityService securityService)
        {
            _uow = uow;
            _uow.CheckArgumentIsNull(nameof(_uow));

            _users = _uow.Set<User>();

            _securityService = securityService;
            _securityService.CheckArgumentIsNull(nameof(_securityService));
        }

        public Task<User> FindUserAsync(Guid userId)
        {
            return _users.FindAsync(userId);
        }

        public Task<User> FindUserAsync(string username, string password)
        {
            var passwordHash = _securityService.GetSha256Hash(password.Trim());
            return _users.FirstOrDefaultAsync(x =>
             x.Username == username &&
             x.Password == passwordHash );
        }
        public async Task<User> FindUserAsync(string username)
        {
            var user=await _users.FirstOrDefaultAsync(u => 
                u.Username.Trim()==username.Trim() && u.IncludeThisRecord).ConfigureAwait(false);
            return user;
        }

        public async Task<string> GetSerialNumberAsync(Guid userId)
        {
            var user = await FindUserAsync(userId).ConfigureAwait(false);
            return user.SerialNumber;
        }
        public async Task<string> ChangeSerialNuberAsync(Guid userId)
        {
            var user=await FindUserAsync(userId).ConfigureAwait(false);
            var newSerialNumber=Guid.NewGuid().ToString("N");
            user.SerialNumber=newSerialNumber;
            return newSerialNumber;
        }
      
        public async Task<ICollection<User>> Get(int take,int skip)
        {
            var users=await _users.OrderBy(u => u.Username).Skip(skip).Take(take).ToListAsync();
            return users;
        }
        public async Task UpdateUserLastActivityDateAsync(Guid userId)
        {
            var user = await FindUserAsync(userId).ConfigureAwait(false);
            if (user.LastLoggedIn != null)
            {
                var updateLastActivityDate = TimeSpan.FromMinutes(2);
                var currentUtc = DateTimeOffset.UtcNow;
                var timeElapsed = currentUtc.Subtract(user.LastLoggedIn.Value);
                if (timeElapsed < updateLastActivityDate)
                {
                    return;
                }
            }
            user.LastLoggedIn = DateTimeOffset.UtcNow;
            await _uow.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task RegisterUserAsync(User user)
        {
            user.Password= _securityService.GetSha256Hash(user.Password.Trim());
            await _users.AddAsync(user);
        }

        public async Task UpdateDeviceSerialAsync(Guid userId,string deviceId)
        {
            var user=await FindUserAsync(userId).ConfigureAwait(false);
            user.DeviceId=deviceId;
        }

        public void UpdateUserAsync(User userInDb,UpdateUserViewModel userViewModel)
        {
            userInDb.DeviceId=userViewModel.deviceId;
            userInDb.DisplayName=userViewModel.DisplayName;
            userInDb.FirstName=userViewModel.FirstName;
            userInDb.LastName=userInDb.LastName;
            userInDb.Mobile=userViewModel.Mobile;
            var newSerialNumber=Guid.NewGuid().ToString("N");
            userInDb.SerialNumber=newSerialNumber;            
        }

        public async Task<bool> CanFindUserAsync(int userCode)
        {
            var user=await _users.FirstOrDefaultAsync(u => u.UserCode==userCode);
            if(user!=null)
            {
                return true;
            }
            return false;
        }
        public async Task<bool> CanFindUserAsync(string lowercaseUsername)
        {
            var user=await _users.FirstOrDefaultAsync(u => u.LowercaseUsername.Trim()==lowercaseUsername.Trim());
            if(user!=null)
            {
                return true;
            }
            return false;
        }
        public async Task<bool> CanFindUserAsync(string lowercaseEmail,bool isEmail)
        {
            var user=await _users.FirstOrDefaultAsync(u => u.LowercaseEmail.Trim()==lowercaseEmail.Trim());
            if(user!=null)
            {
                return true;
            }
            return false;
        }
        
        public async Task<ICollection<User>> FindUsersAsync(IQueryable<UserClaim> userClaims,IQueryable<UserRole> userRoles)
        {
            var userQuery=from user in _users
                join userClaim in userClaims
                on user.Id equals userClaim.UserId
                join userRole in userRoles
                on user.Id equals userRole.UserId
                select user;
            var userList= await userQuery.Distinct().ToListAsync().ConfigureAwait(false);
            return userList;                
        }

        public void ChangeMyPassword(User user,string newPassword)
        {
            user.Password= _securityService.GetSha256Hash(newPassword.Trim());
        }       

        public void FailedLoginAttempt(User user , Policy activePolicy)
        {
            user.InvalidLoginAttemptCount=user.InvalidLoginAttemptCount+1;
            if(user.InvalidLoginAttemptCount>=activePolicy.LockInvalidAttempts)
            {
                user.IsLocked=true;
                user.InvalidLoginAttemptCount=0;
                user.LockTimespan=DateTimeOffset.UtcNow.AddMinutes(activePolicy.LockMin);               
            }          
        } 
        public void SuccessLoginAttempt(User user)
        {
            user.IsLocked=false;
            user.LockTimespan=DateTimeOffset.MinValue;
        }
        public bool CanILogin(User user)
        {
            if(user.IsLocked && user.LockTimespan>=DateTimeOffset.UtcNow)
            {
                return false;
            }
            return true;
        }
    }
}
