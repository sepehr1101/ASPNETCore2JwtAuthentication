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
        Task<ICollection<Claim>> GetClaimsAsync(Guid userId);
        Task<ICollection<UserClaim>> GetUserClaimsAsync(Guid userId);
        IQueryable<UserClaim> GetUserClaimsQuery(string claimType,string claimvalue);
        List<UserClaim> ConvertToClaims(string claimType,ICollection<string> claimValues,Guid insertedBy);
        List<UserClaim> ConvertToClaims(string claimType,ICollection<string> claimValues,Guid insertedBy,Guid userId);
        Task DisablePrviousClaims(Guid userId);
        IQueryable<UserClaim> AddClaimsToQuery(ICollection<string> claims);
    }
    public class ClaimService : IClaimService 
    {
        private readonly IUnitOfWork _uow;
        private readonly DbSet<UserClaim> _userClaims;
        private IQueryable<UserClaim> _userClaimQuery;

        public ClaimService(IUnitOfWork uow)
        {
            _uow=uow;
            _userClaims=_uow.Set<UserClaim>();

            _userClaimQuery=_userClaims;
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
        public async Task<ICollection<Claim>> GetClaimsAsync(Guid userId)
        {
            var query=_userClaims.Where(u=>u.UserId==userId && u.IsActive)
                .Select(x=>new Claim(x.ClaimType,x.ClaimValue));
            var claims=await query.ToListAsync();
            return claims;
        }
        public async Task<ICollection<UserClaim>> GetUserClaimsAsync(Guid userId)
        {
            var query=_userClaims.Where(u=>u.UserId==userId && u.IsActive);
            var userClaims= await query.ToListAsync().ConfigureAwait(false);
            return userClaims;
        }
        public IQueryable<UserClaim> GetUserClaimsQuery(string claimType,string claimvalue)
        {   
            var claims=_userClaims.Where(u => u.ClaimType.Trim()==claimType.Trim() && u.ClaimValue==claimvalue && u.IsActive);
            return claims;
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

        public IQueryable<UserClaim> AddClaimsToQuery(ICollection<string> claims)
        {
            if(claims==null || claims.Count<1)
            {
                return _userClaimQuery;
            }
            _userClaimQuery=_userClaims.Where(u => claims.Contains(u.ClaimValue.Trim()) && u.IsActive);
            return _userClaimQuery;
        }
    }
}