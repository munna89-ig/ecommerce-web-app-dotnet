using System.Security.Cryptography;

namespace Pearl_Poetry.Helpers
{
    public static class OrderNumberGenerator
    {
        public static string Generate()
        {
            string datePart = DateTime.UtcNow.ToString("yyMMdd");

            int randomNumber = RandomNumberGenerator.GetInt32(100000, 1000000);

            return $"PP-{datePart}-{randomNumber}";
        }
    }
}