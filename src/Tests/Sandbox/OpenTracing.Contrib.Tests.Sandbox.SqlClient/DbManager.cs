using Dapper;

namespace OpenTracing.Contrib.Tests.Sandbox.SqlClient
{
    public interface IDbManager
    {
        void CreateDb();
    }

    public class DbManager : IDbManager
    {
        private readonly IDapperContext _dapperContext;

        public DbManager(IDapperContext dapperContext)
        {
            _dapperContext = dapperContext;
        }

        public void CreateDb()
        {
            using (var conn = _dapperContext.Connection)
            {
                conn.ConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB";
                conn.Open();

                var sql = $@"
                    IF EXISTS(select * from sys.databases where name='TestSandbox')
                    DROP DATABASE TestSandbox
                    CREATE DATABASE [TestSandbox]";

                conn.Execute(sql);
            }
        }
    }
}
