using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace PlaySafe.Models
{
    public class specials
    {
        [Key]
        public Guid id { get; set; }
        public string description { get; set; }
        public string photo { get; set; }
        public double price { get; set; }
        public string supervisorId { get; set; }
        [ForeignKey("supervisorId")]
        public ApplicationUser supervisor { get; set; }

    }
}
