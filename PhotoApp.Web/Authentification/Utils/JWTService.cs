using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace PhotoApp.Web.Authentification.Utils
{
    public class JWTService
    {
        public static bool HasTokenExpired(string accessToken, TimeSpan marginForValidation)
        {
            try
            {
                // trim 'Bearer ' from the start since its just a prefix for the token string
                var jwtEncodedString = accessToken.Substring(7);
                var token = new JwtSecurityToken(jwtEncodedString);
                return token.ValidTo <= DateTime.Now.ToUniversalTime() + marginForValidation;
            }
            catch (Exception ex)
            {
                return true;
            }
        }

        public static async Task<string> RequestForNewToken(string userId, string url, string apiEndpoint)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException();

            var json = JsonConvert.SerializeObject(
                new
                {
                    userId = userId
                });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                var response = await client.PostAsync($"{url}/{apiEndpoint}", content);

                //Checking the response is successful or not which is sent using HttpClient  
                if (response.IsSuccessStatusCode)
                {
                    using (HttpContent resContent = response.Content)
                    {
                        return await resContent.ReadAsStringAsync();
                    }
                }
            }

            return null;
        }
    }
}