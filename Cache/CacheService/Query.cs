using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace CacheService
{
    class Query
    {
        static void Main(string[] args)
        {
            
        }
        public Query()
        {
            MSSQLData data = new MSSQLData();
            foreach (var customer in data.Customers)
            {
                Console.WriteLine("CustomerName {0}", customer.CustomerName);
            }

        }

        int Query_Func_1()
        {
            return 0;
        }
    }
}
