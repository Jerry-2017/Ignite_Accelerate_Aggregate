using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;

namespace CacheService
{
    class PreLoad
    {
        static void Main(string[] args)
        {
            // Test 
        }
        public void Process(ICache<int, Customer> cache)
        {
            CustomerDataContext data = new CustomerDataContext();
            int cnt = 0;
            foreach (var customer in data.Customers)
            {
                //Console.WriteLine("CustomerName {0}", customer.CustomerName);
                cache.Put(cnt, customer);
                cnt += 1;
            }
        }
    }
}
