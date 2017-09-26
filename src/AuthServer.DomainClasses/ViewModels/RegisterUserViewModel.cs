using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AuthServer.DomainClasses.ViewModels
{
    public class RegisterUserViewModel
    {
        [Required]
        public int UserCode { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string FirstName { get; set; } 

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "پسوورد و تایید آن با هم مطابقت ندارند")]
        public string PasswordConfirm { get; set; }

        [Required]
        public string DisplayName { get; set; }
        public bool IsActive { get; set; }

        public ICollection<int> RoleIds;
        public ICollection<string> ZoneIds;
        public ICollection<string> ClaimValues;
    }
}