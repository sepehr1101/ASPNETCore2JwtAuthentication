using System;
using System.ComponentModel.DataAnnotations;
namespace AuthServer.DomainClasses.ViewModels
{
    public class LoginInfo
    {
        [Required]
        public string Username { get; set; }  

        [Required]
        public string Password { get; set; }
        public string DeviceId { get; set; }
    }
}