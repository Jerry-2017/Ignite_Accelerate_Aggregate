using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using System.Diagnostics;
using Apache.Ignite.Core.Datastream;

namespace CacheService
{
    class PreLoad
    {
        static void Main(string[] args)
        {
            // Test 
        }
        public void Process(IDataStreamer<long, CustomerAccount_ignite> streamer)
        {
            CustomerAccountsDataContext data = new CustomerAccountsDataContext() ;
            long cnt = 0;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            data.ObjectTrackingEnabled = false;
            foreach (var customer in data.CustomerAccounts)
            {
                var customer_ignite = new CustomerAccount_ignite(customer);
                //Console.WriteLine("CustomerName {0}", customer.CustomerName);
                //cache.Put(cnt, customer_ignite);
                streamer.AddData(cnt, customer_ignite);
                cnt += 1;
                //if (cnt % 1000 == 0)
                //    Console.Write("-");
                //if (cnt == 100000)
                 //   break;
            }
            stopwatch.Stop();
            long elapsed_time = stopwatch.ElapsedMilliseconds;
            Console.WriteLine("Finish Data Loading\n");
            Console.WriteLine("Preload Time on {0} rows using {1} ms", cnt, elapsed_time);
            
        }
    }
}
