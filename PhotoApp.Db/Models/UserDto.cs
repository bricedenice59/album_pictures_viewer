using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoApp.Db.Models
{
    [Table("Users")]
    public class UserDto : DomainObject
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public DateTime DatedJoined { get; set; }
    }
}
