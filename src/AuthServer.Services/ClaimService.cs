using System;
using System.Collections.Generic;
using System.Security.Claims;
using AuthServer.DomainClasses;
using AuthServer.DataLayer.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq; 

namespace AuthServer.Services
{
    public interface IClaimService
    {
        void Add (Guid userId,string claimType, string claimValue);
        ICollection<Claim> GetClaims (Guid userId);
    }
    public class ClaimService : IClaimService 
    {
        private readonly IUnitOfWork _uow;
        private readonly DbSet<UserClaim> _userClaims;

        public ClaimService(IUnitOfWork uow)
        {
            _uow=uow;
            _userClaims=_uow.Set<UserClaim>();
        }
        public void Add(Guid userId,string claimType,string claimValue)
        {          
            var userClaim=new UserClaim();
            userClaim.ClaimType=claimType;
            userClaim.ClaimValue=claimValue;
            userClaim.UserId=userId;
            _userClaims.Add(userClaim);
        }

        public ICollection<Claim> GetClaims(Guid userId)
        {
            var query=_userClaims.Where(u=>u.UserId==userId)
                .Select(x=>new Claim(x.ClaimType,x.ClaimValue));
            return query.ToList();
        }
    }
}