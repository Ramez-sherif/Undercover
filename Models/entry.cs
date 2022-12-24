using System.ComponentModel.DataAnnotations;

namespace PlaySafe.Models
{
    public class entry
    {
        [Key]
        public Guid id { get; set; }
        public int price { get; set; }
        public int points { get; set; }
    }
}
