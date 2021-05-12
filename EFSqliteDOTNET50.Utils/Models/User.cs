using System;

namespace EFSqliteDOTNET50.Utils.Models
{
    public class User : DomainObject
    {
        public string Email { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public DateTime DatedJoined { get; set; }

        public static User Clone(User user)
        {
            return new User()
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
