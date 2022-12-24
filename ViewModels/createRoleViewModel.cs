using System.ComponentModel.DataAnnotations;

namespace PlaySafe.ViewModels
{
    public class createRoleViewModel
    {
        [Required]
        public string RoleName { get; set; }
    }
}
