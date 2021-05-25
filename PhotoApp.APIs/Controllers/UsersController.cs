using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PhotoApp.Db.DbContext;
using PhotoApp.Db.Models;
using PhotoApp.APIs.AuthenticationServices;
using PhotoApp.Utils.Models;

namespace PhotoApp.APIs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IAuthenticationService _authenticationService;

        public UsersController(IAuthenticationService authenticationService, ILogger<UsersController> logger)
        {
            _authenticationService = authenticationService;
            _logger = logger;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("{Login}")]
        public async Task<ActionResult<bool>> Login([FromBody] object credentialsJson)
        {
            if (credentialsJson == null)
                return false;
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
            return authenticationOk;
        }
        
    }
}
