using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Microservice.Framework.RabbitMQ.Extensions
{
    public static class StringExtensions
    {
        private static readonly Regex RegexToSlug = new Regex("(?<=.)([A-Z])", RegexOptions.Compiled);

        public static string ToSlug(this string str)
        {
            return RegexToSlug.Replace(str, "-$0").ToLowerInvariant();
        }

        public static string ToSha256(this string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(bytes);
                return hash
                    .Aggregate(new StringBuilder(), (sb, b) => sb.Append($"{b:x2}"))
                    .ToString();
            }
        }
    }
}
