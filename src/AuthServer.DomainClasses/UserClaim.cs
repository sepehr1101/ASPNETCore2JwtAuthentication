using System;
namespace AuthServer.DomainClasses
{
    public class UserClaim
    {      
        public int Id { get; set; }
        public Guid  UserId { get; set; }
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
        public bool IsActive { get; set; }
        public Guid InsertBy { get; set; }
        public DateTimeOffset InsertTimespan { get; set; }
        public User User { get; set; }

        public UserClaim()
        {            
        }
        public UserClaim(string claimType,string claimValue, Guid insertBy)
        {
            ClaimType=claimType;
            ClaimValue=claimValue;
            IsActive=true;
            InsertBy=insertBy;
        }
    }
}