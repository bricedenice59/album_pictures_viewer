using System;
using System.IdentityModel.Tokens.Jwt;

namespace PhotoApp.Web
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
    }
}