using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using PhotoApp.Utils.Models;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace PhotoApp.APIs.AuthenticationServices
{
    internal class JwtTokenUtils
    {
        public static string GetToken(string userId, string issuer, string audience, string secret, double expirationDelay)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, userId)
            };

            var secretBytes = Encoding.UTF8.GetBytes(secret);
            var key = new SymmetricSecurityKey(secretBytes);
            var algorithm = SecurityAlgorithms.HmacSha256;
            var signingCredentials = new SigningCredentials(key, algorithm);

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                DateTime.Now.ToUniversalTime(),
                DateTime.Now.AddSeconds(expirationDelay).ToUniversalTime(),
                signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        //public static string ValidateJwtToken(string token, string issuer, string audience, string secret)
        //{
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var key = Encoding.ASCII.GetBytes(secret);
        //    try
        //    {
        //        tokenHandler.ValidateToken(token, new TokenValidationParameters
        //        {
        //            ValidAudience = audience,
        //            ValidIssuer = issuer,
        //            ValidateIssuerSigningKey = true,
        //            IssuerSigningKey = new SymmetricSecurityKey(key),
        //            ValidateIssuer = true,
        //            ValidateAudience = true,

        //            ClockSkew = TimeSpan.Zero
        //        }, out SecurityToken validatedToken);

        //        var jwtToken = (JwtSecurityToken)validatedToken;
        //        var accountId = jwtToken.Claims.First(x => x.Type == "jti").Value;

        //        // return user id from JWT token if validation successful
        //        return accountId;
        //    }
        //    catch(Exception ex)
        //    {
        //        //IDX10223: Lifetime validation failed. The token is expired. ValidTo: 'System.DateTime', Current time: 'System.DateTime'.

        //        return null;
        //    }
        //}
    }
}
