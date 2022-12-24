using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace PlaySafe.ViewModels
{
    public class playerViewModel
    {
        [Required]
        [RegularExpression(@"^[A-Za-z]{3,}$", ErrorMessage = "Fullname must be atleast 3 letters and cannot have any illegal characters ex(1,2,.,@ etc..)")]
        public string name { get; set; }
        [Required]
        [Display(Name = "Username")]
        [RegularExpression(@"^[A-Za-z]{3,20}\d{2,5}$", ErrorMessage = "Enter A Valid UserName(Begin with atleast 3 letters then add 2 to 5 numbers)")]
        public string userName { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,}$", ErrorMessage = "Password must be (min 8 characters, atleast one uppercase,one lowercase and one number)")]
        [Display(Name = "Password")]
        public string password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("password",ErrorMessage = "Password and confirmation password do not match")]
        public string confirmPassword { get; set; }
        [Required]
        [RegularExpression(@"^[0-9]{3}-[0-9]{3}-[0-9]{4}$", ErrorMessage = "Please Enter an valid Phone Number(123-123-1234)")]
        public string phoneNum { get; set; }
        public DateTime createdDate { get; set; }
        //[RegularExpression(@"^(([a-zA-Z]:)|(\\{2}\w+)\$?)(\\(\w[\w].*))(.jpg|.JPG|.gif|.GIF|.jpeg|.JPEG|.bmp|.BMP|.png|.PNG)$", ErrorMessage = "You must upload an image")]
        [Required]
        public IFormFile? photo { get; set; }
        public int points { get; set; }
    }
}
