using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace MRP.Persistence
{
    public static class DbConnectionFactory
    {
        
        private const string DefaultConnString =
            "Host=localhost;Port=5432;Username=mrp;Password=mrp;Database=mrp";

        public static NpgsqlConnection CreateOpen()
        {
            var connString = Environment.GetEnvironmentVariable("MRP_DB") ?? DefaultConnString;
            var conn = new NpgsqlConnection(connString);
            conn.Open();
            return conn;
        }
    }
}
