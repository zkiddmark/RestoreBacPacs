using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Dac;

namespace RestoreBacPacs;
internal class DatabaseFacade
{
    private string _connectionString = "Server={0};User Id={1};Password={2};TrustServerCertificate=true";
    public DatabaseFacade(string databaseName, string dbLogin, string dbPwd)
    {
        _connectionString = string.Format(_connectionString, databaseName, dbLogin, dbPwd);
    }

    public async Task CloseConnectionAsync(string database, CancellationToken cancellation) 
    {
        using var connection = new SqlConnection(_connectionString);
        try
        {
            await connection.OpenAsync(cancellation);

            var command = connection.CreateCommand();
            command.CommandText = @$"IF EXISTS (SELECT * FROM [sys].[databases] WHERE [name] = '{database}') ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;";
            await command.ExecuteNonQueryAsync(cancellation);
            await connection.CloseAsync();

        }
        catch (Exception)
        {
            Console.WriteLine("Failed to close connection to {0}", database);
        }
    }

    public async Task DropDatabaseAsync(string database, CancellationToken cancellation)
    {
        using var connection = new SqlConnection(_connectionString);
        try
        {
            await connection.OpenAsync(cancellation);

            var command = connection.CreateCommand();
            command.CommandText = @$"IF EXISTS (SELECT * FROM [sys].[databases] WHERE [name] = '{database}') DROP DATABASE [{database}];";
            await command.ExecuteNonQueryAsync(cancellation);
            await connection.CloseAsync();

        }
        catch (Exception)
        {
            Console.WriteLine("Failed to drop {0}", database);
        }
    }

    public async Task RestoreDatabaseUsingBacpacAsync(string bacpac, CancellationToken cancellation)
    {
        var dacService = new DacServices(_connectionString);
        await Task.Run(() =>
        {
            var fileName = Path.GetFileName(bacpac);
            var database = fileName.Split(".")[0];
            // Import the BACPAC file
            var bacPackage = BacPackage.Load($"{bacpac}");
            dacService.ImportBacpac(bacPackage, database);
            Console.WriteLine("BACPAC file ({0}) imported successfully.", database);
        }, cancellation);
    }
}
