using System.ComponentModel.DataAnnotations;
namespace AuthServer.DomainClasses.ViewModels
{
    public class ChangePasswordViewModel
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }

        [Compare("NewPassword")]
        public string NewPasswordConfirm { get; set; }
    }
}