using Microsoft.Extensions.Logging;
using Nounbase.Core.Interfaces.Factories;
using Nounbase.Core.Models.Schema;
using Nounbase.Services.SqlServer.Interfaces;
using System.Data;

namespace Nounbase.Services.SqlServer.Factories
{
    public class SqlServerSchemaFactory : ISchemaFactory
    {
        private readonly ISqlServerSchemaProvider schemaProvider;
        private readonly ILogger logger;

        public SqlServerSchemaFactory(ISqlServerSchemaProvider schemaProvider, ILogger<SqlServerSchemaFactory> logger)
        {
            ArgumentNullException.ThrowIfNull(schemaProvider, nameof(schemaProvider));
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));

            this.schemaProvider = schemaProvider;
            this.logger = logger;
        }

        public string DatabaseType => "Microsoft SQL Server";

        public async Task<Schema> CreateSchema(string schemaName)
        {
            ArgumentNullException.ThrowIfNull(schemaName, nameof(schemaName));

            try
            {
                logger.LogDebug($"Defining [{DatabaseType}] database schema [{schemaName}]...");

                var schema = new Schema(DatabaseType, schemaName);

                schema.Tables = await DefineTables(schema);
                schema.ForeignKeys = await DefineForeignKeys(schema);

                return schema;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Unable to define [{DatabaseType}] database schema [{schemaName}]. " +
                     "See inner exception for more details.", ex);
            }
        }

        private async Task<List<ForeignKey>> DefineForeignKeys(Schema schema)
        {
            try
            {
                logger.LogDebug($"Defining [{DatabaseType}] database schema [{schema.Name}] foreign keys...");

                var foreignKeys = new List<ForeignKey>();
                var sqlForeignKeys = await schemaProvider.GetForeignKeys(schema.Name!);

                foreach (var sqlForeignKey in sqlForeignKeys)
                {
                    var primaryTable =
                        schema.Tables.FirstOrDefault(t => t.Name == sqlForeignKey.PrimaryKeyTableName!)
                        ?? throw new InvalidOperationException(
                           $"Foreign key primary table [{sqlForeignKey.PrimaryKeyTableName!}] " +
                           $"not found in [{schema.Name}] schema.");

                    var foreignTable =
                        schema.Tables.FirstOrDefault(t => t.Name == sqlForeignKey.ForeignKeyTableName!)
                        ?? throw new InvalidOperationException(
                           $"Foreign key foreign table [{sqlForeignKey.ForeignKeyTableName!}] " +
                           $"not found in [{schema.Name}] schema.");

                    foreignKeys.Add(sqlForeignKey.ToCoreModel());
                }

                return foreignKeys;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Unable to define [{DatabaseType}] database schema [{schema.Name}] foreign keys. " +
                     "See inner exception for more details.", ex);
            }
        }

        private async Task<List<Table>> DefineTables(Schema schema)
        {
            try
            {
                logger.LogDebug($"Defining [{DatabaseType}] database schema [{schema.Name}] tables...");

                var tables = new List<Table>();
                var sqlTables = await schemaProvider.GetTables(schema.Name!);
                var sqlPrimaryKeys = await schemaProvider.GetPrimaryKeys(schema.Name!);

                foreach (var sqlTable in sqlTables)
                {
                    var table = sqlTable.ToCoreModel();

                    table.PrimaryKeys = sqlPrimaryKeys
                         .Where(pkt => pkt.Name == sqlTable.Name)
                         .SelectMany(pkt => pkt.PrimaryKeyColumns.Select(pkc => pkc.Name!))
                         .ToList();

                    tables.Add(table);
                }

                return tables;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Unable to define [{DatabaseType}] database schema [{schema.Name}] tables. " +
                     "See inner exception for more details.", ex);
            }
        }
    }
}
