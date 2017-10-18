using System;

namespace AuthServer.DomainClasses
{
    public class UserRole
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public int RoleId { get; set; }
        public bool IsActive { get; set; }

        public virtual User User { get; set; }
        public virtual Role Role { get; set; }
    }
}