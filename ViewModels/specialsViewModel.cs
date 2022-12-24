using PlaySafe.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PlaySafe.ViewModels
{
    public class specialsViewModel
    {
        [Required]
        public string description { get; set; }
        [Required]
        public IFormFile photo { get; set; }
        [Required]
        public double price { get; set; }
        
        public string? supervisorId { get; set; }
    }
}