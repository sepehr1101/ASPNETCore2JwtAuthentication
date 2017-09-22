using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthServer.Common;
using AuthServer.DataLayer.Context;
using AuthServer.DomainClasses;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Services
{
    public interface IUsersService
    {
        Task<string> GetSerialNumberAsync(Guid userId);
        Task<string> ChangeSerialNuberAsync(Guid userId);
        Task<User> FindUserAsync(string username, string password);
        Task<User> FindUserAsync(Guid userId);
        Task<User> FindUserAsync(string username);
        Task UpdateUserLastActivityDateAsync(Guid userId);
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
    }
}
