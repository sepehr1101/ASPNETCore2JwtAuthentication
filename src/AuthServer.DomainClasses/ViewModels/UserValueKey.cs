using System;
namespace AuthServer.DomainClasses.ViewModels
{
    public class UserValueKey
    {
        public Guid Id { get; set; }
        public int UserCode { get; set; }
        public string FirstName { get; set; } 
        public string LastName { get; set; }
        public string DisplayName { get; set; }
    }
}