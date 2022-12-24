using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace PlaySafe.Models
{
    public class comments
    {
        [Key]
        public Guid id { get; set; }
        [DisplayName("Comment")]
        [Required]
        public string comment { get; set; }
        public string userId { get; set; }
        [ForeignKey("userId")]
        public ApplicationUser user { get; set; }
       
    }
}
