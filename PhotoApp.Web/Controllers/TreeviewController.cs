using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PhotoApp.Utils;
using PhotoApp.Utils.Models;
using PhotoApp.Web.Authentification.Utils;

namespace PhotoApp.Web.Controllers
{
    public class TreeviewController : Controller
    {
        private readonly ILogger<TreeviewController> _logger;
        private readonly IHttpContextAccessor _httpContext;

        private const string Baseurl = "https://localhost:4000";
        private const string ApiAlbums = "api/Albums/";
        private const string ApiGetRefreshToken = "api/Auth/GetRefreshedToken";
        private const long CookieExpiration = TimeSpan.TicksPerDay * 7; //7 days validity

        public TreeviewController(IHttpContextAccessor httpContext, ILogger<TreeviewController> logger)
        {
            _httpContext = httpContext;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = Request.Cookies["X-Access-Token"];
            if (token == null)
                return Redirect("../Home/Index");

            if (JWTService.HasTokenExpired(token, TimeSpan.FromSeconds(30)))
            {
                string tokenResult = null;
                try
                {
                    tokenResult =
                        await JWTService.RequestForNewToken(Request.Cookies["X-Access-User"], Baseurl, ApiGetRefreshToken);
                }
                catch (HttpRequestException e)
                {
                    ViewBag.ErrorMessage = $"Web API {Baseurl}/{ApiGetRefreshToken} is unavailable";
                    return View("Index");
                }

                if (!string.IsNullOrEmpty(tokenResult))
                {
                    token = "Bearer" + " " + tokenResult;
                    //delete the old variable cookie
                    Response.Cookies.Delete("X-Access-Token");
                    Response.Cookies.Append("X-Access-Token", tokenResult,
                        new CookieOptions()
                        {
                            Expires = DateTime.Now.AddTicks(CookieExpiration),
                            HttpOnly = true,
                            SameSite = SameSiteMode.Strict
                        });
                }
                else
                {
                    HttpContext.Session.SetString("IsLogged", false.ToString());
                    View("Index");
                }
            }

            Dictionary<string, string> albums;
            try
            {
                albums = await GetAllAlbums();
            }
            catch (HttpRequestException e)
            {
                ViewBag.ErrorMessage = $"Web API {Baseurl}/{ApiAlbums} is unavailable";
                return View("Index");
            }

            var treeviewStructure = TreeviewUtils.FindRootNode(albums);
            if (treeviewStructure != null)
            {
                foreach (KeyValuePair<string, string> item in albums)
                {
                    string value = item.Value;
                    if (value.Contains("/"))
                    {
                        string[] splittedValues = value.Split("/", StringSplitOptions.RemoveEmptyEntries);
                        if (splittedValues.Length != 0)
                        {
                            TreeviewUtils.FillTree(ref treeviewStructure,
                                splittedValues.Where((string x) => x != treeviewStructure.Header).ToList());
                        }
                    }
                }
            }
            return View(new TreeviewViewModel(){AlbumsFolders = treeviewStructure.Children });
        }

        private async Task<Dictionary<string, string>> GetAllAlbums()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Request.Cookies["X-Access-Token"]);

                var response = await client.GetAsync($"{Baseurl}/{ApiAlbums}");
                if (response.IsSuccessStatusCode)
                {
                    using (HttpContent resContent = response.Content)
                    {
                        var jsonResponse = await resContent.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(jsonResponse))
                        {
                            return  
                                JsonConvert.DeserializeAnonymousType(jsonResponse, new[] { new { Id = "", Path = "" }})!
                                    .Select(x=>x)
                                    .ToDictionary(y=>y.Id, z=>z.Path);
                            
                        }
                    }
                }
            }

            return null;
        }
    }
}
