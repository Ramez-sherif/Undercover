using System.ComponentModel.DataAnnotations;

namespace PlaySafe.ViewModels
{
    public class loginViewModel
    {
        [Required]
        public string userName { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string password { get; set; }
        [Display(Name = "Remember Me")]
        public bool rememberMe { get; set; }
    }
}
