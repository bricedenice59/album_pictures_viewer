using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoApp.Db.Models
{
    [Table("Users")]
    public class UserDto : DomainObject
    {
        public string Email { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public DateTime DatedJoined { get; set; }

        public static UserDto Clone(UserDto user)
        {
            return new UserDto()
            {
                Id = user.Id,
                Username = user.Username,
                PasswordHash = user.PasswordHash,
                DatedJoined = user.DatedJoined,
                Email = user.Email
            };
        }
    }
}
