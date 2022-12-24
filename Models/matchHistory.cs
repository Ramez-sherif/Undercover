using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;

namespace PlaySafe.Models
{
    public class matchHistory
    {
        [Key]
        public Guid id { get; set; }
        public string userId { get; set; }
        [ForeignKey("userId")]
        public ApplicationUser user { get; set; }
        public Guid? entryId { get; set; }
        [ForeignKey("entryId")]
        public entry? entry { get; set; }
        public DateTime createdDate { get; set; } = DateTime.Now;
        public bool active { get; set; }
        public bool withPoints { get; set; }
        public int? customPrice { get; set; }
    }
}
