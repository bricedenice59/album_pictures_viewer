using System;
using System.Threading.Tasks;
using PhotoApp.Db.Models;

namespace PhotoApp.APIs.AuthenticationServices
{
    public interface IAuthenticationService
    {
        /// <summary>
        /// Get an account for a user's credentials.
        /// </summary>
        /// <param name="username">The user's name.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>The account for the user.</returns>
        /// <exception cref="UserNotFoundException">Thrown if the user does not exist.</exception>
        /// <exception cref="InvalidPasswordException">Thrown if the password is invalid.</exception>
        /// <exception cref="Exception">Thrown if the login fails.</exception>
        Task<UserDto> Login(string username, string password);
    }
}
