using Microsoft.Extensions.Logging;
using Nounbase.Core.Interfaces.Builders;
using Nounbase.Core.Interfaces.Readers;
using Nounbase.Core.Interfaces.Samplers;
using Nounbase.Core.Models;
using Nounbase.Core.Models.Schema;
using Nounbase.Core.Models.Semantic;
using System.Text;

namespace Nounbase.Services.SqlServer.Samplers
{
    public class SqlServerNounSampler : INounSampler
    {
        private const int sampleSize = 25;

        private readonly IDbRecordSetReader dbRecordSetReader;
        private readonly ILogger logger;
        private readonly IQueryBuilder queryBuilder;

        public SqlServerNounSampler(
            IDbRecordSetReader dbRecordSetReader, 
            ILogger<SqlServerNounSampler> logger,
            IQueryBuilder queryBuilder)
        {
            ArgumentNullException.ThrowIfNull(dbRecordSetReader, nameof(dbRecordSetReader));
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
            ArgumentNullException.ThrowIfNull(queryBuilder, nameof(queryBuilder));

            this.dbRecordSetReader = dbRecordSetReader;
            this.logger = logger;
            this.queryBuilder = queryBuilder;
        }

        public async Task<DbRecordSet?> GetNounSampleSet(Noun noun, Understanding understanding)
        {
            ArgumentNullException.ThrowIfNull(noun, nameof(noun));
            ArgumentNullException.ThrowIfNull(understanding,  nameof(understanding));

            try
            {
                logger.LogInformation($"Trying to get [{sampleSize}] [{noun.Name}] sample(s)...");

                if (noun.Properties.Any())
                {
                    var query = queryBuilder.BuildSampleQuery(noun, understanding);

                    return await dbRecordSetReader.GetRecordSet(query);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get [{noun.Name}] samples. See inner exception for more details.", ex);
            }
        }
    }
}
