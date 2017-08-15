using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheService
{
    static class Helper
    {
        [ThreadStatic]
        static Random random = null;


        private static void EnsureRandom()
        {
            if (random == null)
            {
                random = new Random();
            }
        }

        public static long GetRandomAccountID(List<long> accountIDs)
        {
            EnsureRandom();
            return accountIDs[random.Next(accountIDs.Count)];
        }

        public static IEnumerable<long> GetRandomAccountIDs(List<long> accountIDs, int howMany)
        {
            EnsureRandom();
            List<long> result = new List<long>();
            for (int i = 0; i < howMany; i++)
            {
                result.Add(accountIDs[random.Next(accountIDs.Count)]);
            }
            return result;
        }

        public static string GetRandomString(int length)
        {
            EnsureRandom();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
