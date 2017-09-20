using System;
namespace AuthServer.DomainClasses
{
    public class Login
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public int BrowserId { get; set; }
        public DateTimeOffset LoginTimespan { get; set; }
        public string LoginIP { get; set; }
        public bool WasSuccessful { get; set; }

        public virtual Browser Browser { get; set; }
        public virtual User User{get;set;}
    }
}