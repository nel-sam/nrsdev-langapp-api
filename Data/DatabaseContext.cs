using System.Data;
using Microsoft.Data.Sqlite;

namespace Data;

/* 
    Why a factory rather than a single shared connection? 
    SqliteConnection isn't thread-safe and ASP.NET handles requests 
    concurrently. Creating a short-lived connection per operation is 
    the idiomatic Dapper pattern, using it so it's disposed when done.
*/
public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}

public class SqlLiteConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        var connection = new SqliteConnection(connectionString);
        connection.Open();
        return connection;
    }
}