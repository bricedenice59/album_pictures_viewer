using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using PhotoApp.Db.DbContext;
using PhotoApp.Db.Models;
using PhotoApp.APIs.AuthenticationServices;
using PhotoApp.Utils;
using PhotoApp.Utils.Models;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace PhotoApp.APIs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IAuthenticationService _authenticationService;

        public UsersController(IConfiguration configuration, 
            IAuthenticationService authenticationService,
            ILogger<UsersController> logger)
        {
            _configuration = configuration;
            _authenticationService = authenticationService;
            _logger = logger;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("{Login}")]
        public async Task<IActionResult> Login([FromBody] object credentialsJson)
        {
            if (credentialsJson == null)
                return Unauthorized();

            var obj = JsonConvert.DeserializeObject<User>(credentialsJson.ToString());
            bool authenticationOk = false;
            UserDto userToLogin = null;
            try
            {
                userToLogin = await _authenticationService.Login(obj.UserName, obj.Password);
                authenticationOk = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            var result = new LoginResult()
            {
                IsSuccessful = authenticationOk,
                UserId = AesUtils.EncryptString(userToLogin?.UserId),
                Token = authenticationOk 
                    ? AesUtils.EncryptString(JwtTokenUtils.GetToken(userToLogin?.UserId,
                        _configuration["Auth0:Issuer"],
                        _configuration["Auth0:Audience"],
                        _configuration["Auth0:Secret"],
                Convert.ToDouble(_configuration["Auth0:TokenExpirationDelay"])))
                    : null
            };
            return Ok(result);
        }
    }
}
