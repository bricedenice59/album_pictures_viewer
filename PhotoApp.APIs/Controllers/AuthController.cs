using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PhotoApp.APIs.AuthenticationServices;
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

        public AuthController(IConfiguration configuration, 
            IAuthenticationService authenticationService,
            ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _authenticationService = authenticationService;
            _logger = logger;
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("GetRefreshedToken")]
        private IActionResult GetRefreshedToken([FromBody] string token)
        {
            // trim 'Bearer ' from the start since its just a prefix for the token string
            if (token.Contains("Bearer "))
                token = token.Substring(7);

            //first is it a valid token ?
            var idTokenUser = JwtTokenUtils.ValidateJwtToken(token, _configuration["Auth0:Secret"]);
            if (idTokenUser == null)
                return null;

            return Ok(token);
        }
    }
}
