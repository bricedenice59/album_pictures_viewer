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
        private readonly TreeviewViewModel _treeviewModel;

        private const string Baseurl = "https://localhost:4000";
        private const string ApiAlbums = "api/Albums";
        private const string ApiPhotos = "api/Photos";
        private const string ApiGetRefreshToken = "api/Auth/GetRefreshedToken";
        private const long CookieExpiration = TimeSpan.TicksPerDay * 7; //7 days validity

        public TreeviewController(TreeviewViewModel treeviewModel, IHttpContextAccessor httpContext, ILogger<TreeviewController> logger)
        {
            _treeviewModel = treeviewModel;
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

            if (albums == null)
                return NoContent();

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
                            TreeviewUtils.FillTree(ref treeviewStructure, item.Key,
                                splittedValues.Where((string x) => x != treeviewStructure.Header).ToList());
                        }
                    }
                }
            }

            _treeviewModel.AlbumsFolders = treeviewStructure;
            return View(_treeviewModel);
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

        [HttpGet]
        public async Task<IActionResult> GetPhotosFromAlbum(string albumId, string pageNumber)
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

            List<TreeviewViewModel.PhotoModel> photos;
            try
            {
                photos = await GetPhotosFromAlbumAsync(Convert.ToInt32(albumId), Convert.ToInt32(pageNumber));
            }
            catch (HttpRequestException e)
            {
                ViewBag.ErrorMessage = $"Web API {Baseurl}/{ApiPhotos} is unavailable";
                return View("Index");
            }

            if (photos == null)
                return NoContent();

            foreach (var photo in photos)
            {
                var data = Convert.ToBase64String(photo.Thumbnail);
                photo.ImgDataURL = $"data:image/png;base64,{data}";
            }

            _treeviewModel.PhotosList = photos;
            return PartialView("~/Views/Partial/PhotosList.cshtml", _treeviewModel.PhotosList);
        }


        private async Task<List<TreeviewViewModel.PhotoModel>> GetPhotosFromAlbumAsync(int albumId, int pageNumber)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Request.Cookies["X-Access-Token"]);
                var request = $"{Baseurl}/{ApiPhotos}/GetPhotosForAlbumId?albumId={albumId}&pageNumber={pageNumber}";
                var response = await client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    using (HttpContent resContent = response.Content)
                    {
                        var jsonResponse = await resContent.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(jsonResponse))
                        {
                            return JsonConvert.DeserializeObject<List<TreeviewViewModel.PhotoModel>>(jsonResponse);
                        }
                    }
                }
            }

            return null;
        }
    }
}
