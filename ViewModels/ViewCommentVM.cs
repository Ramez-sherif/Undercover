using PlaySafe.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace PlaySafe.ViewModels
{
    public class ViewCommentVM
    {
        [DisplayName("Comment")]
        [Required]
        public string comment { get; set; }
        public string name { get; set; }
        public string userId { get; set; }
    }
}
