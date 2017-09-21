using System;
using System.Collections.Generic;
using System.Security.Claims;
using AuthServer.DomainClasses;
using AuthServer.DataLayer.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq; 

namespace AuthServer.Services
{
    public interface ILoginService
    {
        void Add(Login login);
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
    }
}