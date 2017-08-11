using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Query;
using Apache.Ignite.Linq;
using Apache.Ignite.Core.Cache.Configuration;

namespace CacheService
{
    class Query
    {
        static void Main(string[] args)
        {
            
        }
        public void Process(ICache<long, CustomerAccount_ignite> cache,long id)
        {
            //var que = cache.AsCacheQueryable();
            //IQueryable<ICacheEntry<long, CustomerAccount>> 
            var queryable = cache.AsCacheQueryable( );

            var accounts = queryable.Where(customer => customer.Value._AccountId==id).ToArray();

            //var sql = new SqlQuery(typeof(CustomerAccount_ignite), "where _AccountId == ?", id);

            //IQueryCursor<ICacheEntry<long, CustomerAccount_ignite>> accounts = cache.Query(sql);

            foreach (var account in accounts)
            {
                Console.WriteLine("{0} {1} {2}", account.Value.AccountId, account.Value.AccountName, account.Value.CustomerName);
            }

            /*
            foreach ( var cust in cache.Query(new ScanQuery<int, CustomerAccount>()))
            {
                var entry = cust.Value;
                Console.WriteLine("{0} {1} {2}", entry.CustomerId,entry.CustomerName,entry.CustomerNumber);
            }
            //var table = que.Where( x => true).ToArray();*/

        }

        int Query_Func_1()
        {
            return 0;
        }
    }
}
