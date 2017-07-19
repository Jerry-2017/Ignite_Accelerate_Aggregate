using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Query;
using Apache.Ignite.Linq;

namespace CacheService
{
    class Query
    {
        static void Main(string[] args)
        {
            
        }
        public void Process(ICache<int, Customer> cache)
        {
            //var que = cache.AsCacheQueryable();
            foreach ( var cust in cache.Query(new ScanQuery<int, Customer>()))
            {
                var entry = cust.Value;
                Console.WriteLine("{0} {1} {2}", entry.CustomerId,entry.CustomerName,entry.CustomerNumber);
            }
            //var table = que.Where( x => true).ToArray();

        }

        int Query_Func_1()
        {
            return 0;
        }
    }
}
