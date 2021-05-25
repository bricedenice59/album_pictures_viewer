using System.ComponentModel.DataAnnotations;

namespace PhotoApp.Utils.Models
{
    public class User
    {
        [Required(ErrorMessage = "Please enter your User ID.")]
        public string UserId { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Please enter your Password.")]
        public string Password { get; set; }
    }
}