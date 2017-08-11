using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Transactions;
using System.Configuration;
using Apache.Ignite.Core.Cluster;
using System.Diagnostics;

namespace CacheService
{
    class Program
    {
        static void Main(string[] args)
        {
            //string conn_string = ConfigurationManager.ConnectionStrings["CacheService.Properties.Settings.testConnectionString"].Name;
            Ignition.ClientMode = true;
            using (IIgnite ignite = Ignition.StartFromApplicationConfiguration())
            {

                ICluster cluster = ignite.GetCluster();
                ICollection<IClusterNode> t = cluster.ForRemotes().GetNodes();
                List< IClusterNode> t1=t.ToList<IClusterNode>();
                Console.WriteLine("{0}", t1.ToString());
                PreLoad preload = new PreLoad();
                Query query = new Query();

                var cache = ignite.GetOrCreateCache<long, CustomerAccount_ignite>(new CacheConfiguration("Cache", typeof(CustomerAccount_ignite)));//"Cache"
                preload.Process(cache);

                long id = 1;
                int times = 1000000;
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                for (int i=0;i<times;i++)
                {
                    query.Process(cache,id);
                    id = (id + 3) % 100000;
                }
                stopwatch.Stop();
                long elapsed_time = stopwatch.ElapsedMilliseconds;
                Console.WriteLine("Finish SQL querying\n");
                Console.WriteLine("Preload Time on {0} times using {1} ms", times, elapsed_time);
                //var cache = ignite.GetOrCreateCache<int, string>("myCache");
                // Store keys in cache (values will end up on different cache nodes).
                /*for (int i = 0; i < 10; i++)
                    cache.Put(i, i.ToString());

                for (int i = 0; i < 10; i++)
                    Console.WriteLine("Got [key={0}, val={1}]", i, cache.Get(i));*/
            }
        }
    }
}
