using System;
using System.Collections.Generic;

namespace AuthServer.DomainClasses.ViewModels
{
    public class UserDisplayViewModel
    {
        public Guid Id { get; set; }
        public int UserCode { get; set; }
         
        public string Username { get; set; }
         
        public string FirstName { get; set; } 
         
        public string LastName { get; set; }
         
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string DeviceId { get; set; }  
        public string Mobile { get; set; }
        public bool IsActive { get; set; }
    }
}