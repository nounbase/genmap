using Nounbase.Core.Interfaces.Readers;
using Nounbase.Core.Models.Schema;
using Nounbase.Services.SqlServer.Constants;
using System.Data;
using System.Data.SqlClient;
using static Nounbase.Core.Utilities.Environment;

namespace Nounbase.Services.SqlServer.Readers
{
    public class SqlServerDbRecordSetReader : BaseSqlServerDbReader, IDbRecordSetReader
    {
        private readonly string dbConnectionString;

        public SqlServerDbRecordSetReader()
            : this(GetRequiredEnvironmentVariable(NounbaseEnv.DbConnectionString)) { }

        public SqlServerDbRecordSetReader(string dbConnectionString) =>
            this.dbConnectionString = dbConnectionString
            ?? throw new ArgumentNullException(nameof(dbConnectionString));

        public async Task<DbRecordSet> GetRecordSet(string sqlQuery)
        {
            ArgumentNullException.ThrowIfNull(sqlQuery, nameof(sqlQuery));

            try
            {
                await Semaphore.WaitAsync();

                using (var sqlConnection = new SqlConnection(dbConnectionString))
                using (var sqlCommand = new SqlCommand(sqlQuery, sqlConnection))
                {
                    await sqlConnection.OpenAsync();

                    var recordSet = new DbRecordSet();

                    using (var sqlReader = await sqlCommand.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        recordSet.ColumnNames = new string[sqlReader.FieldCount];

                        for (int fieldIndex = 0; fieldIndex < sqlReader.FieldCount; fieldIndex++)
                        {
                            recordSet.ColumnNames[fieldIndex] = sqlReader.GetName(fieldIndex);
                        }

                        var rows = new List<string[]>();

                        while (await sqlReader.ReadAsync())
                        {
                            var row = new string[sqlReader.FieldCount];

                            for (int valueIndex = 0; valueIndex < sqlReader.FieldCount; valueIndex++)
                            {
                                var value = sqlReader.GetValue(valueIndex).ToString();

                                if (value != null)
                                {
                                    row[valueIndex] = value;
                                }
                            }

                            rows.Add(row);
                        }

                        recordSet.Rows = rows.ToArray();
                    }

                    return recordSet;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to get record set from SQL Server using query [{sqlQuery}].", ex);
            }
            finally
            {
                Semaphore.Release();
            }
        }
    }
}
