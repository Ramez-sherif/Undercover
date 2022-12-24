using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlaySafe.Models
{
    public class player
    {
        [Key]
        public Guid Id { get; set; }
        public string userId { get; set; }
        [ForeignKey("userId")]
        public ApplicationUser user { get; set; }
        [Required]
        public string photo { get; set; }
        public int points { get; set; } = 0;
    }
}
