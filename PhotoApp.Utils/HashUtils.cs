using System;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace PhotoApp.Utils
{
    public class HashUtils
    {
        private const string Salt = "m'A#8QAUJt9mYcR-EYvn`t";

        public static string Generate(string value)
        {
            byte[] salt = Encoding.UTF8.GetBytes("Salt".ToCharArray());

            // derive a 256-bit subkey (use HMACSHA1 with 10,000 iterations)
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: value,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
        }
    }
}
