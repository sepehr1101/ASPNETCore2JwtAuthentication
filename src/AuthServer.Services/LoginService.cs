using System;
using System.Collections.Generic;
using System.Security.Claims;
using AuthServer.DomainClasses;
using AuthServer.DataLayer.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq; 
using System.Threading.Tasks;

namespace AuthServer.Services
{
    public interface ILoginService
    {
        void Add(Login login);
        Task<ICollection<Login>> GetUserLogins(Guid userId);
    }
    public class LoginService:ILoginService
    {
        private readonly IUnitOfWork _uow;
        private readonly DbSet<Login> _logins;
        public LoginService(IUnitOfWork uow)
        {
            _uow=uow;
            _logins=_uow.Set<Login>();
        }
        public void Add(Login login)
        {
            _logins.Add(login);
        }
        public async Task<ICollection<Login>> GetUserLogins(Guid userId)
        {
            var logins=await _logins.Where(u => u.UserId==userId)
                .OrderByDescending(u => u.LoginTimespan)
                .ToListAsync();
            return logins;
        }
    }
}