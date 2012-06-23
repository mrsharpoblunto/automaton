using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AutomatonLib
{
    public static class Cryptography
    {
        public static string GenerateTimestamp(DateTime messageTimestamp)
        {
            return messageTimestamp.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        }

        public static string GenerateHMac(string password, DateTime messageTimestamp)
        {
            return GenerateHMac(password, GenerateTimestamp(messageTimestamp));
        }

        public static string GenerateHMac(string password, string messageTimestamp)
        {
            HMACSHA256 hmacsha256 = new HMACSHA256(UTF8Encoding.UTF8.GetBytes(password));
            byte[] computedHash = hmacsha256.ComputeHash(UTF8Encoding.UTF8.GetBytes(messageTimestamp));
            return Convert.ToBase64String(computedHash);
        }
    }
}
