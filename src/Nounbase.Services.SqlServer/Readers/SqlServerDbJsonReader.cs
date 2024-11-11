using Microsoft.Extensions.Logging;
using Nounbase.Core.Interfaces.Readers;
using Nounbase.Services.SqlServer.Constants;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.Json;
using static Nounbase.Core.Utilities.Environment;

namespace Nounbase.Services.SqlServer.Readers
{
    public class SqlServerDbJsonReader : BaseSqlServerDbReader, IDbJsonReader
    {
        private readonly string dbConnectionString;

        public SqlServerDbJsonReader()
            : this(GetRequiredEnvironmentVariable(NounbaseEnv.DbConnectionString))
        { }

        public SqlServerDbJsonReader(string dbConnectionString) =>
            this.dbConnectionString = dbConnectionString
            ?? throw new ArgumentNullException(nameof(dbConnectionString));

        public async Task<T> Get<T>(string sqlQuery)
        {
            ArgumentNullException.ThrowIfNull(sqlQuery, nameof(sqlQuery));

            return JsonSerializer.Deserialize<T>(await GetJson(sqlQuery))!;
        }

        public async Task<IList<T>> GetList<T>(string sqlQuery)
        {
            ArgumentNullException.ThrowIfNull(sqlQuery, nameof(sqlQuery));

            return JsonSerializer.Deserialize<List<T>>(await GetJson(sqlQuery))!;
        }

        private async Task<string> GetJson(string sqlQuery)
        {
            try
            {
                await Semaphore.WaitAsync();

                using (var sqlConnection = new SqlConnection(dbConnectionString))
                using (var sqlCommand = new SqlCommand(sqlQuery, sqlConnection))
                {
                    await sqlConnection.OpenAsync();

                    // Apparently SQL Server will chunk the JSON response into individual rows
                    // if the response is too long...

                    using (var sqlReader = await sqlCommand.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        var builder = new StringBuilder();

                        while (await sqlReader.ReadAsync())
                        {
                            builder.Append(sqlReader.GetString(0));
                        }

                        return builder.ToString()!;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to get JSON from SQL Server using query [{sqlQuery}].", ex);
            }
            finally
            {
                Semaphore.Release();
            }
        }
    }
}
