using System.ComponentModel.DataAnnotations;

namespace PhotoApp.Utils.Models
{
    public class HomeViewModel
    {
        public User User { get; set; }
        public File File { get; set; }
    }

    public class User
    {
        [Required(ErrorMessage = "Please enter your User ID.")]
        public string UserId { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Please enter your Password.")]
        public string Password { get; set; }
    }


    public class File
    {
        [Required(ErrorMessage = "Please enter a filename")]
        public string Filename { get; set; }
    }
}