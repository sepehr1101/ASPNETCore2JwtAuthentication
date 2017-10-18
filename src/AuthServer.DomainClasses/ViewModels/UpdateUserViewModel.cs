using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AuthServer.DomainClasses.ViewModels
{
    public class UpdateUserViewModel
    {       
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string FirstName { get; set; } 

        [Required]
        public string LastName { get; set; }
        [Required]
        public string DisplayName { get; set; }

        public string deviceId { get; set; }  

        [Required]
        [MaxLength(11)]
        [MinLength(11)]
        public string Mobile { get; set; }
        public bool IsActive { get; set; }

        public ICollection<int> RoleIds;
        public ICollection<string> ZoneIds;
        public ICollection<string> Actions;
    }
}