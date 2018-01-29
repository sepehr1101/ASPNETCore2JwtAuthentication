using System;
using System.Collections.Generic;
using System.Security.Claims;
using AuthServer.DomainClasses;
using AuthServer.DomainClasses.ViewModels;
using AuthServer.DataLayer.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq; 
using System.Threading.Tasks;

namespace AuthServer.Services
{
    public interface IPolicyService
    {
        Task<Policy> FindFirstAsync();
        Policy FindActive();
        Task<Policy> FindActiveAsync();
        Task UpdatePasswordPolicy(PasswordPolicy passwordPolicy);
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

        public Policy FindActive()
        {
            var policy =  _policies.FirstOrDefault(p => p.IsActive);
            return policy;
        }
        public async Task<Policy> FindActiveAsync()
        {
            var policy = await _policies.FirstOrDefaultAsync(p => p.IsActive);
            return policy;
        }

        public async Task UpdatePasswordPolicy(PasswordPolicy passwordPolicy)
        {
            var activePolicy=await FindActiveAsync();
            activePolicy.PasswordContainsLowercase=passwordPolicy.PasswordContainsLowercase;
            activePolicy.PasswordContainsNonAlphaNumeric=passwordPolicy.PasswordContainsNonAlphaNumeric;
            activePolicy.PasswordContainsNumber=passwordPolicy.PasswordContainsNumber;
            activePolicy.PasswordContainsUppercase=passwordPolicy.PasswordContainsUppercase;;
            activePolicy.MinPasswordLength=passwordPolicy.MinPasswordLength;
        }
    }
}