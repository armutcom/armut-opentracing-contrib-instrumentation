using System.Data;
using System.Data.SqlClient;

namespace OpenTracing.Contrib.Tests.Sandbox.SqlClient
{
    public class DapperContext : IDapperContext
    {
        private readonly string _connectionString;
        private IDbConnection _connection;

        public DapperContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    _connection = new SqlConnection(_connectionString);
                }

                if (string.IsNullOrEmpty(_connection.ConnectionString))
                {
                    _connection.ConnectionString = _connectionString;
                }

                return _connection;
            }
        }
    }

    public interface IDapperContext
    {
        IDbConnection Connection { get; }
    }
}
