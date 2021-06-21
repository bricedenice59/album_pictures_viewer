using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
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
        private readonly TreeviewViewModel _treeviewModel;

        private const string ApiAlbums = "api/Albums";
        private const string ApiPhotos = "api/Photos";
        private const string ApiGetRefreshToken = "api/Auth/GetRefreshedToken";
        private readonly int _cookieExpiration;
        private readonly string Baseurl = null;

        public TreeviewController(TreeviewViewModel treeviewModel,
            ILogger<TreeviewController> logger,
            IConfiguration configuration)
        {
            _treeviewModel = treeviewModel;
            _logger = logger;

            _cookieExpiration = configuration.GetValue<int>("CookieExpiration");
            Baseurl = configuration.GetValue<string>("UrlWebAPi");
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Request.Cookies["X-Access-Token"];
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
                            Expires = DateTime.Now.AddTicks(_cookieExpiration),
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

            List<AlbumModelDto> albums;
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
                foreach (var item in albums)
                {
                    if (item.Path.Contains("\\"))
                    {
                        string[] splittedValues = item.Path.Split("\\", StringSplitOptions.RemoveEmptyEntries);
                        if (splittedValues.Length != 0)
                        {
                            TreeviewUtils.FillTree(ref treeviewStructure, 
                                item.Id.ToString(), 
                                item.NbPhotos,
                                splittedValues.Where((string x) => x != treeviewStructure.Header).ToList());
                        }
                    }
                }
            }

            _treeviewModel.AlbumsFolders = treeviewStructure;
            return View(_treeviewModel);
        }

        private async Task<List<AlbumModelDto>> GetAllAlbums()
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
                            return JsonConvert.DeserializeObject<List<AlbumModelDto>>(jsonResponse);
                        }
                    }
                }
            }

            return null;
        }

        [HttpGet]
        public async Task<IActionResult> GetPhotosFromAlbum(string albumId, string pageNumber, string nbPhotosForAlbum)
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
                            Expires = DateTime.Now.AddTicks(_cookieExpiration),
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

            PhotosModelDto photosModelDto;
            try
            {
                photosModelDto = await GetPhotosFromAlbumAsync(
                    Convert.ToInt32(albumId),
                    Convert.ToInt32(pageNumber),
                    Convert.ToInt32(nbPhotosForAlbum));
            }
            catch (HttpRequestException e)
            {
                ViewBag.ErrorMessage = $"Web API {Baseurl}/{ApiPhotos} is unavailable";
                return View("Index");
            }

            if (photosModelDto == null)
                return NoContent();

            _treeviewModel.PhotosModel = new PhotosModelDto(photosModelDto.ListPhotos);
            return PartialView("~/Views/Partial/PhotosList.cshtml", _treeviewModel.PhotosModel.ListPhotos);
        }


        private async Task<PhotosModelDto> GetPhotosFromAlbumAsync(int albumId, int pageNumber, int nbPhotoFromAlbum)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Request.Cookies["X-Access-Token"]);
                var request = $"{Baseurl}/{ApiPhotos}/GetPhotosForAlbumId?albumId={albumId}&pageNumber={pageNumber}&nbPhotosForAlbumId={nbPhotoFromAlbum}";
                var response = await client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    using (HttpContent resContent = response.Content)
                    {
                        var jsonResponse = await resContent.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(jsonResponse))
                        {
                            return JsonConvert.DeserializeObject<PhotosModelDto>(jsonResponse);
                        }
                    }
                }
            }

            return null;
        }
    }
}
