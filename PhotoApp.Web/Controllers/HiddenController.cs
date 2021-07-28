using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PhotoApp.Web.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PhotoApp.Utils;
using PhotoApp.Utils.Models;
using PhotoApp.Web.Authentification.Utils;

namespace PhotoApp.Web.Controllers
{
    public class HiddenController : Controller
    {
        private readonly ILogger<HiddenController> _logger;

        private const string ApiGetRefreshToken = "api/Auth/GetRefreshedToken";
        private int _cookieExpiration;

        public HiddenController(ILogger<HiddenController> logger, 
            IConfiguration configuration)
        {
            _logger = logger;
            _cookieExpiration = configuration.GetValue<int>("CookieExpiration");
        }

        public IActionResult Index()
        {
            string tokenInCookie = Request.Cookies["X-Access-Token"];

            //no cookie exists ? then display login page by setting session variable IsUserConnected = false;
            bool cookieExist = !string.IsNullOrEmpty(tokenInCookie);

            if (!cookieExist)
            {
                HttpContext.Session.SetString("IsUserConnected", false.ToString());
                return Redirect("../Home/Index");
            }
            return View("../Hidden/Index");
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
