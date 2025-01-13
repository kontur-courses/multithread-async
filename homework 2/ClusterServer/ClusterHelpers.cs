using System;
using System.Security.Cryptography;
using System.Text;

namespace Cluster
{
    internal static class ClusterHelpers
    {
        public static byte[] GetBase64HashBytes(string query)
        {

            using (var hasher = new HMACMD5(Key))
            {
                var hash = Convert.ToBase64String(hasher.ComputeHash(encoding.GetBytes(query ?? "")));
                return encoding.GetBytes(hash);
            }
        }

        private static readonly Encoding encoding = Encoding.UTF8;
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("Контур.Шпора");
    }

}
