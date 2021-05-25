using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PhotoApp.Web.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using PhotoApp.Utils.Models;

namespace PhotoApp.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPasswordHasher<User> _passwordHasher;

        //private const string Baseurl = "http://bgcode.synology.me:7500/";
        private const string Baseurl = "https://localhost:50995";
        private const string ApiAuthentification = "api/Users/Login";

        public HomeController(IPasswordHasher<User> passwordHasher, ILogger<HomeController> logger)
        {
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index([Bind] User user)
        {
            //validate your model    
            if (user == null || !ModelState.IsValid)
            {
                return BadRequest();
            }

            user.Password = _passwordHasher.HashPassword(user, user.Password);

            var json = JsonConvert.SerializeObject(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync($"{Baseurl}/{ApiAuthentification}", content);

                //Checking the response is successful or not which is sent using HttpClient  
                if (response.IsSuccessStatusCode)
                {
                    using (HttpContent resContent = response.Content)
                    {
                        var jsonResponse = await resContent.ReadAsStringAsync();
                        return View();
                    }
                }
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
