using System;

namespace AuthServer.DomainClasses
{
    public class UserToken
    {
        public int Id { get; set; }

        public string AccessTokenHash { get; set; }

        public DateTimeOffset AccessTokenExpiresDateTime { get; set; }

        public string RefreshTokenIdHash { get; set; }

        public DateTimeOffset RefreshTokenExpiresDateTime { get; set; }

        public Guid UserId { get; set; } // one-to-one association
        public virtual User User { get; set; }
    }
}