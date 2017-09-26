using System;

namespace AuthServer.DomainClasses.ViewModels
{
    public class RegisterUserViewModel
    {
        public int UserCode { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; } 
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordConfirm { get; set; }
        public string DisplayName { get; set; }
        public bool IsActive { get; set; }

    }
}