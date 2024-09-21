using System.Data;
using System.Data.SqlClient;
using Dapper;
namespace FoodOrderApi
{
   public class DBHelper
{
    private readonly string _connectionString;

    public DBHelper(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection Connection => new SqlConnection(_connectionString);
}
}


