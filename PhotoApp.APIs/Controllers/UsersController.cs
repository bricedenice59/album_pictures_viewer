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
        public async Task<IActionResult> Login([FromBody] object credentialsJson)
        {
            if (credentialsJson == null)
                return Unauthorized();

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
                Token = authenticationOk == true 
                    ? JwtTokenUtils.GetToken(obj, _configuration["Auth0:Issuer"], _configuration["Auth0:Audience"],_configuration["Auth0:Secret"]) 
                    : null
            };
            return Ok(result);
        }
    }
}
