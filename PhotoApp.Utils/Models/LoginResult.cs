namespace PhotoApp.Utils.Models
{
    public class LoginResult
    {
        public bool IsSuccessful { get; set; }
        public string Token { get; set; }
        public string UserId { get; set; }
    }
}