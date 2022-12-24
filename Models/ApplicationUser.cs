using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.Identity;

namespace PlaySafe.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [DisplayName("Name")]
        public string name { get; set; }

        [DisplayName("Account Creatation Date")]
        public DateTime createdDate { get; set; } = DateTime.Now;
        public string? supervisorId { get; set; }
        [ForeignKey("supervisorId")]
        public ApplicationUser? supervisor { get; set; }
        public bool? status { get; set; }

    }
}
