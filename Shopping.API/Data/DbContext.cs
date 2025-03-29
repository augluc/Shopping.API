﻿using System.Data;
using Microsoft.Data.SqlClient;


namespace Shopping.API.Data
{
    public class DbContext
    {
        private readonly IConfiguration _configuration;

        public DbContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDbConnection CreateConnection()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("The connection string isn't configurated.");
            }
            return new SqlConnection(connectionString);
        }
    }
}
