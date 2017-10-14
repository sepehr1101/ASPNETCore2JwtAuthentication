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
    public interface IPolicyService
    {
        Task<Policy> FindFirstAsync();
    }
    public class PolicyService:IPolicyService
    {
        private readonly IUnitOfWork _uow;
        private readonly DbSet<Policy> _policies;
        public PolicyService(IUnitOfWork uow)
        {
            _uow=uow;
            _policies=_uow.Set<Policy>();
        }
        public async Task<Policy> FindFirstAsync()
        {
            var policy=await _policies.FindAsync().ConfigureAwait(false);
            return policy;
        }
    }
}