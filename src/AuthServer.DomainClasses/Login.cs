using System;
namespace AuthServer.DomainClasses
{
    public class Login
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public int? BrowserId { get; set; }
        public DateTimeOffset LoginTimespan { get; set; }
        public string LoginIP { get; set; }
        public bool WasSuccessful { get; set; }
        public int? OsId { get; set; }
        public string BrowserVersion { get; set; }
        public string OsVersion { get; set; }

        public virtual Browser Browser { get; set; }
        public virtual User User{get;set;}
        public virtual OS OS{get;set;}
    }
}