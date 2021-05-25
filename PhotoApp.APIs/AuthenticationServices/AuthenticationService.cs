using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PhotoApp.Db.DbContext;
using PhotoApp.Db.Models;
using PhotoApp.Utils.Exceptions;

namespace PhotoApp.APIs.AuthenticationServices
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly AppDbContextFactory _dbContextFactory;
        private readonly IPasswordHasher<UserDto> _passwordHasher;

        public AuthenticationService(AppDbContextFactory dbContextFactory, IPasswordHasher<UserDto> passwordHasher)
        {
            _dbContextFactory = dbContextFactory;
            _passwordHasher = passwordHasher;
        }

        public async Task<UserDto> Login(string username, string password)
        {
            UserDto storedUser = null;
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                storedUser = await dbContext.Users.
                    FirstOrDefaultAsync(x => x.Username == username 
                                             && x.PasswordHash == password);
            }

            if(storedUser == null)
            {
                throw new UserNotFoundException(username);
            }

            var passwordResult = _passwordHasher.VerifyHashedPassword(storedUser,storedUser.PasswordHash, password);

            if(passwordResult != PasswordVerificationResult.Success)
            {
                throw new InvalidPasswordException(username, password);
            }

            return storedUser;
        }
    }
}
