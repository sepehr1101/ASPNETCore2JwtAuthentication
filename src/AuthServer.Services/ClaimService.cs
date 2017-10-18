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
    public interface IClaimService
    {
        void Add (Guid userId,string claimType, string claimValue);
        Task AddRangeAsync(ICollection<UserClaim> userClaims);
        ICollection<Claim> GetClaims (Guid userId);
        List<UserClaim> ConvertToClaims(string claimType,ICollection<string> claimValues,Guid insertedBy);
        List<UserClaim> ConvertToClaims(string claimType,ICollection<string> claimValues,Guid insertedBy,Guid userId);
        Task DisablePrviousClaims(Guid userId);
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
        public async Task AddRangeAsync(ICollection<UserClaim> userClaims)
        {
            await _userClaims.AddRangeAsync(userClaims);
        }
        public ICollection<Claim> GetClaims(Guid userId)
        {
            var query=_userClaims.Where(u=>u.UserId==userId)
                .Select(x=>new Claim(x.ClaimType,x.ClaimValue));
            return query.ToList();
        }

        public List<UserClaim> ConvertToClaims(string claimType,ICollection<string> claimValues,Guid insertedBy)
        {
            var claims=new List<UserClaim>();
            if(claimValues!=null && claimValues.Count>0)
            {
                foreach(var claimValue in claimValues)
                {
                    var claim=new UserClaim(claimType,claimValue,insertedBy);
                    claim.InsertTimespan=DateTime.UtcNow;
                    claims.Add(claim);
                }
                return claims;
            }
            return null;
        }

        public List<UserClaim> ConvertToClaims(string claimType,ICollection<string> claimValues,Guid insertedBy,Guid userId)
        {
            var claims=new List<UserClaim>();
            if(claimValues!=null && claimValues.Count>0)
            {
                foreach(var claimValue in claimValues)
                {
                    var claim=new UserClaim(claimType,claimValue,insertedBy);
                    claim.InsertTimespan=DateTime.UtcNow;
                    claim.UserId=userId;
                    claims.Add(claim);
                }
                return claims;
            }
            return null;
        }
        public async Task DisablePrviousClaims(Guid userId)
        {
            var userClaims=await _userClaims.Where(u => u.UserId==userId)
                .ToListAsync();
            foreach (var userClaim in userClaims)
            {
                userClaim.IsActive=false;
            }
        }
    }
}