using System;
namespace AuthServer.DomainClasses.ViewModels
{
    public class LoginViewModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTimeOffset LoginTimespan { get; set; }
        public string LoginIP { get; set; }
        public bool WasSuccessful { get; set; }
        public string BrowserVersion { get; set; }
        public string OsVersion { get; set; }
        public string OsTitle { get; set; }        
        public string BrowserTitle { get; set; }
    }
}