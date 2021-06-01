using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PhotoApp.Web.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using PhotoApp.Utils;
using PhotoApp.Utils.Models;
using PhotoApp.Web.Authentification.Utils;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace PhotoApp.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContext;

        //private const string Baseurl = "http://bgcode.synology.me:7500";
        private const string Baseurl = "https://localhost:4000";
        private const string ApiLoginAuthentification = "api/Users/Login";
        private const string ApiGetRefreshToken = "api/Auth/GetRefreshedToken";
        private const long CookieExpiration = TimeSpan.TicksPerDay * 7; //7 days validity

        public HomeController(IHttpContextAccessor httpContext, ILogger<HomeController> logger)
        {
            _httpContext = httpContext;
            _logger = logger;
        }

        public IActionResult Index()
        {
            string tokenInCookie = Request.Cookies["X-Access-Token"];

            //no cookie exists ? then display login page by setting session variable IsUserConnected = false;
            bool cookieExist = !string.IsNullOrEmpty(tokenInCookie);
            
            HttpContext.Session.SetString("IsUserConnected", cookieExist.ToString());
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckFile([Bind] HomeViewModel homeViewModel)
        {
            //validate your model    
            if (homeViewModel == null || !ModelState.IsValid)
            {
                return View("Index");
            }

            var token = _httpContext.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (JWTService.HasTokenExpired(token, TimeSpan.FromSeconds(30)))
            {
                var tokenResult = await JWTService.RequestForNewToken(Request.Cookies["X-Access-User"], Baseurl, ApiGetRefreshToken);
                if (!string.IsNullOrEmpty(tokenResult))
                {
                    token = "Bearer" + " " + tokenResult;
                    //delete the old variable cookie
                    Response.Cookies.Delete("X-Access-Token");
                    Response.Cookies.Append("X-Access-Token", tokenResult,
                        new CookieOptions() { Expires = DateTime.Now.AddTicks(CookieExpiration), HttpOnly = true, SameSite = SameSiteMode.Strict });
                }
                else
                {
                    HttpContext.Session.SetString("IsLogged", false.ToString());
                    return View("Index");
                }
            }

            var fileDto = homeViewModel.File;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", token);

                var response = await client.GetAsync($"{Baseurl}/api/photos/{fileDto.Filename}");
                if (response.IsSuccessStatusCode)
                {
                    using (HttpContent resContent = response.Content)
                    {
                        var jsonResponse = await resContent.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(jsonResponse))
                        {
                        }
                    }
                }
            }
            return View("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind] HomeViewModel homeViewModel)
        {
            //validate your model    
            if (homeViewModel == null || !ModelState.IsValid)
            {
                return View("Index");
            }

            var userDto = homeViewModel.User;

            userDto.Password = HashUtils.Generate(userDto.Password);

            var json = JsonConvert.SerializeObject(userDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                HttpResponseMessage response = null;
                try
                {
                    response = await client.PostAsync($"{Baseurl}/{ApiLoginAuthentification}", content);
                }
                catch (HttpRequestException e)
                {
                    ViewBag.ErrorMessage = $"Web API {Baseurl}/{ApiLoginAuthentification} is unavailable";
                    return View("Index");
                }

                //Checking the response is successful or not which is sent using HttpClient  
                if (response.IsSuccessStatusCode)
                {
                    using (HttpContent resContent = response.Content)
                    {
                        var jsonResponse = await resContent.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(jsonResponse))
                        {
                           var result = JsonConvert.DeserializeObject<LoginResult>(jsonResponse);
                           if (result.IsSuccessful)
                           {
                               Response.Cookies.Append("X-Access-Token", result.Token, new CookieOptions()
                               {
                                   Expires = DateTime.Now.AddTicks(CookieExpiration), HttpOnly = true, SameSite = SameSiteMode.Strict
                               });
                               Response.Cookies.Append("X-Access-User", result.UserId, new CookieOptions()
                               {
                                   Expires = DateTime.Now.AddTicks(CookieExpiration), HttpOnly = true, SameSite = SameSiteMode.Strict
                               });
                               HttpContext.Session.SetString("IsUserConnected", result.IsSuccessful.ToString());
                           }
                           else ModelState.AddModelError("", "The user name or password provided is incorrect.");
                        }
                    }
                }
            }

            return View("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
