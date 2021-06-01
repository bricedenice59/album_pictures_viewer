using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PhotoApp.APIs.AuthenticationServices;
using PhotoApp.Db.DbContext;
using PhotoApp.Db.Models;
using PhotoApp.Utils;
using PhotoApp.Utils.Models;

namespace PhotoApp.APIs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IAuthenticationService _authenticationService;
        private readonly AppDbContextFactory _dbContextFactory;

        public AuthController(IConfiguration configuration, 
            IAuthenticationService authenticationService,
            AppDbContextFactory dbContextFactory,
            ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _authenticationService = authenticationService;
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("GetRefreshedToken")]
        public async Task<IActionResult> GetRefreshedToken([FromBody] object userObj)
        {
            if (userObj == null)
                return Unauthorized();

            var definition = new { userId = "" };

            var userCookieObj = JsonConvert.DeserializeAnonymousType(userObj.ToString(), definition);
            var user = AesUtils.DecryptString(userCookieObj.userId);

            //decryption failed ?
            if(user == null)
                return Unauthorized();

            //is the user passed from cookie a valid user that is saved in database ?
            bool userFound = false;
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                try
                {
                    userFound = dbContext.Users
                        .Any(x => x.UserId == user);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }
            if(!userFound)
                return Unauthorized();

            return Ok(
                AesUtils.EncryptString(JwtTokenUtils.GetToken(
                 userCookieObj.userId, 
                _configuration["Auth0:Issuer"], 
                _configuration["Auth0:Audience"],
                _configuration["Auth0:Secret"],
            Convert.ToDouble(_configuration["Auth0:TokenExpirationDelay"])))
            );
        }
    }
}
