using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PhotoApp.Db.DbContext;
using PhotoApp.Db.Models;
using PhotoApp.Utils.Exceptions;
using PhotoApp.Utils.Models;

namespace PhotoApp.APIs.AuthenticationServices
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly AppDbContextFactory _dbContextFactory;

        public AuthenticationService(AppDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
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

            var passwordResult = storedUser.PasswordHash.Equals(password);
            if (!passwordResult)
            {
                throw new InvalidPasswordException(username, password);
            }

            return storedUser;
        }
    }
}
