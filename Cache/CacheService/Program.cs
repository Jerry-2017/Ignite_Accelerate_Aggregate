using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Threading.Tasks;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Linq;
using Apache.Ignite.Core.Datastream;
using System.Configuration;
using Apache.Ignite.Core.Cluster;
using System.Diagnostics;
using System.Threading;

namespace CacheService
{
    class Program
    {
        static IIgnite ignite;
        static ICache<long, CustomerAccount_ignite> cache;
        static List<long> accountIDs = new List<long>();

        const int numWriteThread = 20;
        const int numReadThread = 20;
        const int numDuration = 1; // minutes

        static long writeTotalLatency = 0L;
        static long writeCalls = 0L;
        static long writeSucceeded = 0L;
        static long writeFailed = 0L;

        static long totalLatency = 0L;
        static long calls = 0L;
        static long succeeded = 0L;
        static long failed = 0L;

        static void PopulateAccountIDs()
        {

            var queryable = cache.AsCacheQueryable();

            var accounts = queryable.Select(customer => new { customer.Value._AccountId}).ToArray();
            foreach (var account in accounts)
            {
                accountIDs.Add(account._AccountId);
            }
        }

        private static DataTable CreateDataTable(IEnumerable<long> ids)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Id", typeof(long));
            foreach (long id in ids.Distinct())
            {
                table.Rows.Add(id);
            }
            return table;
        }

        private static void Reset()
        {
            totalLatency = 0L;
            calls = 0L;
            succeeded = 0L;
            failed = 0L;
        }

        static void Main(string[] args)
        {
            //string conn_string = ConfigurationManager.ConnectionStrings["CacheService.Properties.Settings.testConnectionString"].Name;
            Ignition.ClientMode = true;
            using (ignite = Ignition.StartFromApplicationConfiguration())
            {

                ICluster cluster = ignite.GetCluster();
                ICollection<IClusterNode> t = cluster.ForRemotes().GetNodes();
                List<IClusterNode> t1 = t.ToList<IClusterNode>();
                Console.WriteLine("{0}", t1.ToString());
                PreLoad preload = new PreLoad();
                String CacheName = "Cache";
                var caches = ignite.GetCacheNames();
                cache = ignite.GetOrCreateCache<long, CustomerAccount_ignite>(
                    new CacheConfiguration(CacheName, typeof(CustomerAccount_ignite)) { CacheMode = CacheMode.Partitioned }
                );//"Cache"
                IDataStreamer<long, CustomerAccount_ignite> streamer = ignite.GetDataStreamer<long, CustomerAccount_ignite>("Cache");
                streamer.AllowOverwrite = true;
                IQueryable<ICacheEntry<long, CustomerAccount_ignite>> queryable = cache.AsCacheQueryable();

                if (!caches.Contains(CacheName))
                    preload.Process(streamer);

                
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " Populating Account IDs");
                PopulateAccountIDs();
                Console.WriteLine(accountIDs.Count);
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " Start writing tasks");

                CancellationTokenSource writeTokenSource = new CancellationTokenSource();
                CancellationToken writeToken = writeTokenSource.Token;
                List<Task> writeTasks = new List<Task>();

                for (int i = 0; i < numWriteThread; i++)
                {

                    writeTasks.Add(new Task(() =>
                    {
                        Thread.Sleep(2000);
                        ///Console.Write("tasks1 start ", i);
                        while (!writeToken.IsCancellationRequested)
                        {
                            Stopwatch sw = Stopwatch.StartNew();
                            try
                            {
                                long p = Helper.GetRandomAccountID(accountIDs);
                                var accounts = queryable.Where(customer => customer.Value._AccountId ==p).ToArray();
                                foreach (var account in accounts)
                                {
                                    account.Value._AgencyName = Helper.GetRandomString(20);
                                    streamer.AddData(account.Key, account.Value);
                                }  
                                Interlocked.Increment(ref writeSucceeded);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                                Interlocked.Increment(ref writeFailed);
                            }
                            sw.Stop();
                            Interlocked.Add(ref writeTotalLatency, sw.ElapsedMilliseconds);
                            Interlocked.Increment(ref writeCalls);
                        }
                    }, writeToken));
                }

                Parallel.ForEach(writeTasks, task => task.Start());

                {
                    CancellationTokenSource tokenSource = new CancellationTokenSource();
                    CancellationToken token = tokenSource.Token;
                    List<Task> tasks = new List<Task>();

                    for (int i = 0; i < numReadThread; i++)
                    {
                        tasks.Add(new Task(() =>
                        {
                            Thread.Sleep(2000);
                            //Console.Write("{0} tasks start" ,i);
                            while (!token.IsCancellationRequested)
                            {
                                Stopwatch sw = Stopwatch.StartNew();
                                try
                                {
                                    long p = Helper.GetRandomAccountID(accountIDs);
                                    var accounts = queryable.Where(customer => customer.Value._AccountId == p).ToArray();
                                    foreach (var account in accounts)
                                    {
                                        //
                                    }
                                    Interlocked.Increment(ref succeeded);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex);
                                    Interlocked.Increment(ref failed);
                                }

                                sw.Stop();
                                Interlocked.Add(ref totalLatency, sw.ElapsedMilliseconds);
                                Interlocked.Increment(ref calls);

                            }
                        }, token));
                    }

                    Parallel.ForEach(tasks, task => task.Start());

                    Thread.Sleep(numDuration * 1000 * 60);
                    tokenSource.Cancel();

                    Console.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    Console.WriteLine(" Reads:");
                    Console.WriteLine("averge latency: " + 1.0 * totalLatency / calls + " ms");
                    Console.WriteLine("throughput: {0} calls/sec", 1.0 * calls / (numDuration * 60));
                    Console.WriteLine("success rate: {0}, success: {1}, failed: {2}, calls: {3}", 1.0 * succeeded / calls, succeeded, failed, calls);
                }

                Reset();

                {
                    CancellationTokenSource tokenSource = new CancellationTokenSource();
                    CancellationToken token = tokenSource.Token;
                    List<Task> tasks = new List<Task>();

                    for (int i = 0; i < numReadThread; i++)
                    {
                        tasks.Add(new Task(() =>
                        {
                            Thread.Sleep(2000);
                            while (!token.IsCancellationRequested)
                            {
                                Stopwatch sw = Stopwatch.StartNew();
                                try
                                {
                                    long p = Helper.GetRandomAccountID(accountIDs);
                                    var accounts = queryable.Where(customer => customer.Value._AccountId == p).ToArray();
                                    foreach (var account in accounts)
                                    {
                                        //
                                    }
                                    Interlocked.Increment(ref succeeded);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex);
                                    Interlocked.Increment(ref failed);
                                }

                                sw.Stop();
                                Interlocked.Add(ref totalLatency, sw.ElapsedMilliseconds);
                                Interlocked.Increment(ref calls);

                            }
                        }, token));
                    }

                    Parallel.ForEach(tasks, task => task.Start());

                    Thread.Sleep(numDuration * 1000 * 60);
                    tokenSource.Cancel();

                    Console.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    Console.WriteLine(" Reads:");
                    Console.WriteLine("averge latency: " + 1.0 * totalLatency / calls + " ms");
                    Console.WriteLine("throughput: {0} calls/sec", 1.0 * calls / (numDuration * 60));
                    Console.WriteLine("success rate: {0}, success: {1}, failed: {2}, calls: {3}", 1.0 * succeeded / calls, succeeded, failed, calls);
                }
                writeTokenSource.Cancel();

                Console.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                Console.WriteLine(" Writes:");
                Console.WriteLine("averge latency: " + 1.0 * writeTotalLatency / writeCalls + " ms");
                Console.WriteLine("throughput: {0} calls/sec", 1.0 * writeCalls / (numDuration * 60));
                Console.WriteLine("success rate: {0}, success: {1}, failed: {2}, calls: {3}", 1.0 * writeSucceeded / writeCalls, writeSucceeded, writeFailed, writeCalls);
                /*long id = 1;
               int times = 10000;
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
              var cache = ignite.GetOrCreateCache<int, string>("myCache");*/
                // Store keys in cache (values will end up on different cache nodes).
                /*for (int i = 0; i < 10; i++)
                    cache.Put(i, i.ToString());

                for (int i = 0; i < 10; i++)
                    Console.WriteLine("Got [key={0}, val={1}]", i, cache.Get(i));*/
            }
        }
    }
}
