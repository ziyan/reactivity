using System;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;

namespace Reactivity.Data
{
    /// <summary>
    /// Database Connection
    /// </summary>
    public class Connection : IDisposable
    {
        /// <summary>
        /// Construct a new database connection
        /// The connection string is read from configuration file
        /// </summary>
        public Connection()
        {
            connection = new SqlConnection(
                System.Configuration.ConfigurationManager.ConnectionStrings[
                    System.Configuration.ConfigurationManager.AppSettings[
                        "Reactivity.Data.Connection"]].ConnectionString);
            connection.Open();
        }

        public void Dispose()
        {
            if (connection != null && connection.State == ConnectionState.Open)
            {
                connection.Close();
                connection.Dispose();
            }
            connection = null;
        }

        internal SqlConnection connection = null;
    }
}
