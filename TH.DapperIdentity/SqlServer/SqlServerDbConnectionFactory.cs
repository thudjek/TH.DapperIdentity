using Microsoft.Data.SqlClient;
using System.Data.Common;
using TH.DapperIdentity.Contracts;

namespace TH.DapperIdentity.SqlServer;

public class SqlServerDbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;
    public SqlServerDbConnectionFactory(string connectionString)
    {

        _connectionString = connectionString;

    }
    public DbConnection Create()
    {
        var sqlConnection = new SqlConnection(_connectionString);
        sqlConnection.Open();
        return sqlConnection;
    }
}
