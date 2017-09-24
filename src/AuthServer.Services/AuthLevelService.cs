using System;
using System.Collections.Generic;
using System.Security.Claims;
using AuthServer.Common;
using AuthServer.DomainClasses;
using AuthServer.DomainClasses.ViewModels;
using AuthServer.DataLayer.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq; 
using System.Threading.Tasks;

namespace AuthServer.Services
{
    public interface IAuthLevelService
    {
        Task<ICollection<A1>> GetAuthLevelsAsync();
    }
    public class AuthLevelService : IAuthLevelService 
    {
        private readonly IUnitOfWork _uow;
        private readonly DbSet<AuthLevel1> _authLevel1s;
        private readonly DbSet<AuthLevel2> _authLevel2s;
        private readonly DbSet<AuthLevel4> _authLevel4s;
        private readonly DbSet<AuthLevel3> _authLevel3s;
        private readonly DbSet<UserClaim> _userClaims;

        public AuthLevelService(IUnitOfWork uow)
        {
            _uow=uow;
            _uow.CheckArgumentIsNull(nameof(_uow));

            _userClaims=_uow.Set<UserClaim>();
            _authLevel1s=_uow.Set<AuthLevel1>();
            _authLevel2s=_uow.Set<AuthLevel2>();
            _authLevel3s=_uow.Set<AuthLevel3>();
            _authLevel4s=_uow.Set<AuthLevel4>();    
        }
       
        public async Task<ICollection<A1>> GetAuthLevelsAsync()
        {
            var authSummeryList=_uow.ExecSQL<AuthSummary>(GetAuthSummaryQuery());
            var authTree=GetAuthTree(authSummeryList);
            return authTree;
        }

        public ICollection<A1> GetAuthTree(ICollection<AuthSummary> authSummary)
        {
            ICollection<A1> a1s=new List<A1>();
            var a1Ids=authSummary.Select(a =>a._1Id).Distinct().ToList();
            foreach(var a1Id in a1Ids)
            {
                var a1First=authSummary.First(a=>a._1Id==a1Id);
                var a1=new A1();
                a1.Id=a1First._1Id;
                a1.Title=a1First._1Title;
                ICollection<A2> a2s=new List<A2>();
                var a2Ids=authSummary.Where(a =>a._1Id==a1Id).Select(a => a._2Id).Distinct().ToList();
                foreach(var a2Id in a2Ids)
                {
                    var a2First=authSummary.First(a =>a._2Id==a2Id);
                    var a2=new A2();
                    a2.Id=a2First._2Id;
                    a2.Title=a2First._2Title;
                    a2.IconClass=a2First.IconClass;
                    ICollection<A3> a3s=new List<A3>();
                    var a3Ids=authSummary
                        .Where(a =>a._1Id==a1Id && a._2Id==a2Id).Select(a => a._3Id).Distinct().ToList();
                    foreach(var a3Id in a3Ids)
                    {
                        var a3First=authSummary.First(a =>a._3Id==a3Id);
                        var a3=new A3();
                        a3.Id=a3First._3Id;
                        a3.Title=a3First._3Title;
                        ICollection<A4> a4s=new List<A4>();
                        var a4Ids=authSummary
                            .Where(a =>a._1Id==a1Id && a._2Id==a2Id && a._3Id==a3Id).Select(a => a._4Id).Distinct().ToList();
                        foreach(var a4Id in a4Ids)
                        {
                            var a4First=authSummary.First(a =>a._4Id==a4Id);
                            var a4=new A4();
                            a4.Id=a4First._4Id;
                            a4.Title=a4First._4Title;           
                            a4.ClaimValue=a4First.ClaimValue;                 
                            a4s.Add(a4);
                        }
                        a3.A4s=a4s;
                        a3s.Add(a3);
                    }
                    a2.A3s=a3s;
                    a2s.Add(a2);
                }              
                a1.A2s=a2s;
                a1s.Add(a1);
            }
            return a1s;
        }
    
    
      private string GetAuthSummaryQuery()
      {
          var query=@"select 
            a1.Id as _1Id,
            a2.Id as _2Id,
            a3.Id as _3Id,
            a4.Id as _4Id,
            a1.AppBoundaryTitle as _1Title,	
            a2.Title as _2Title,
            a3.Title as _3Title,
            a4.Title as _4Title,
            a2.IconClass,
            a4.Value as ClaimValue
            from AuthLevel1s a1
            join AuthLevel2s a2
            on a1.Id=a2.AuthLevel1Id
            join AuthLevel3s a3
            on a2.Id=a3.AuthLevel2Id
            join AuthLevel4s a4
            on a3.Id=a4.AuthLevel3Id";
            return query;
      }

    }    
}