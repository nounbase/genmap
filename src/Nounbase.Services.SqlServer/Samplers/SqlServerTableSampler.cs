using Microsoft.Extensions.Logging;
using Nounbase.Core.Interfaces.Readers;
using Nounbase.Core.Interfaces.Samplers;
using Nounbase.Core.Models;
using Nounbase.Core.Models.Schema;
using System.Data;

namespace Nounbase.Services.SqlServer.Samplers
{
    public class SqlServerTableSampler : ITableSampler
    {
        private const int defaultSampleSize = 25;

        private readonly IDbRecordSetReader dbRecordSetReader;
        private readonly ILogger logger;

        public SqlServerTableSampler(IDbRecordSetReader dbRecordSetReader, ILogger<SqlServerTableSampler> logger)
        {
            ArgumentNullException.ThrowIfNull(dbRecordSetReader, nameof(dbRecordSetReader));
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));

            this.dbRecordSetReader = dbRecordSetReader;
            this.logger = logger;
        }

        public Task<DbRecordSet> GetTableSampleSet(Table table, Understanding understanding, int? sampleSize = null)
        {
            ArgumentNullException.ThrowIfNull(table, nameof(table));
            ArgumentNullException.ThrowIfNull(understanding, nameof(understanding));

            sampleSize ??= defaultSampleSize;

            try
            {
                logger.LogInformation($"Trying to get [{sampleSize}] [{table.Name}] sample(s)...");

                var selectColumns = string.Join(',', table.Columns.Select(c => $"[{table.Name}].[{c.Name}]"));

                var sqlQuery = $@"
                    SELECT      TOP {sampleSize}
                                {selectColumns}
                    FROM        [{understanding.Schema!.Name}].[{table.Name}]
                    ORDER BY    newid()"; // As random as we can get

                return dbRecordSetReader.GetRecordSet(sqlQuery);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unable to obtain [{table.Name}] samples. See inner exception for more details.", ex);
            }
        }
    }
}
