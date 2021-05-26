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
        public async Task<ActionResult<string>> Login([FromBody] object credentialsJson)
        {
            if (credentialsJson == null)
                return JsonConvert.SerializeObject(new LoginResult() { IsSuccessful = false });

            var obj = JsonConvert.DeserializeObject<User>(credentialsJson.ToString());
            bool authenticationOk = false;
            try
            {
                await _authenticationService.Login(obj.UserId, obj.Password);
                authenticationOk = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            var result = new LoginResult()
            {
                IsSuccessful = authenticationOk,
                Token = authenticationOk == true ? GetToken(obj) : null
            };
            return JsonConvert.SerializeObject(result);
        }

        private string GetToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var secretBytes = Encoding.UTF8.GetBytes(_configuration["Auth0:Secret"]);
            var key = new SymmetricSecurityKey(secretBytes);
            var algorithm = SecurityAlgorithms.HmacSha256;
            var signingCredentials = new SigningCredentials(key, algorithm);

            var token = new JwtSecurityToken(
                _configuration["Auth0:Issuer"],
                _configuration["Auth0:Audience"],
                claims,
                DateTime.Now,
                DateTime.Now.AddDays(1),
                signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
