using System;

namespace ASPNETCore2JwtAuthentication.DomainClasses
{
    public class UserClaim
    {      
        public int Id { get; set; }
        public Guid  UserId { get; set; }
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
        public User User { get; set; }
    }
}