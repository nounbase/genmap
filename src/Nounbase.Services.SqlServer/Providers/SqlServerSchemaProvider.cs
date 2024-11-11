using Microsoft.Extensions.Logging;
using Nounbase.Core.Interfaces.Readers;
using Nounbase.Services.SqlServer.Interfaces;

namespace Nounbase.Services.SqlServer.Providers
{
    public class SqlServerSchemaProvider : ISqlServerSchemaProvider
    {
        // This is basically a layer of SQL Server-specific glue that sits betweeen
        // the database itself and the schema factory. Schema factory class was too cluttered
        // with all this SQL and I kind of like the idea of having all SQL contained in
        // one class (SQL modified in one small place) and higher-order logic in the factory.
    
        private readonly IDbJsonReader dbJsonReader;
        private readonly ILogger logger;

        public SqlServerSchemaProvider(IDbJsonReader dbJsonReader, ILogger<SqlServerSchemaProvider> logger)
        {
            ArgumentNullException.ThrowIfNull(dbJsonReader, nameof(dbJsonReader));
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));

            this.dbJsonReader = dbJsonReader;
            this.logger = logger;
        }

        public Task<IList<Models.Source.ForeignKeys.ForeignKey>> GetForeignKeys(string schemaName)
        {
            ArgumentNullException.ThrowIfNull(schemaName, nameof(schemaName));

            try
            {
                logger.LogDebug($"Getting schema [{schemaName}] foreign keys...");

                var sqlQuery = $@"
                    SELECT      fk.[object_id] as constraint_id,
                                fk.[name] as constraint_name,
                                ft.[name] as fk_table_name,
                                fc.[name] as fk_column_name,
                                pt.[name] as pk_table_name,
                                pc.[name] as pk_column_name
                    FROM        sys.foreign_key_columns fkc
                    INNER JOIN  sys.foreign_keys fk
                                ON  fkc.[constraint_object_id] = fk.[object_id]
                    INNER JOIN  sys.tables ft
                                ON  fkc.[parent_object_id] = ft.[object_id]
                    INNER JOIN  sys.tables pt
                                ON  fkc.[referenced_object_id] = pt.[object_id]
                    INNER JOIN  sys.schemas schemas
                                ON  ft.[schema_id] = schemas.[schema_id]
                                AND pt.[schema_id] = schemas.[schema_id]
                    INNER JOIN  sys.columns fc
                                ON  fkc.[parent_object_id] = fc.[object_id]
                                AND fkc.[parent_column_id] = fc.[column_id]
                    INNER JOIN  sys.columns pc
                                ON  fkc.[referenced_object_id] = pc.[object_id]
                                AND fkc.[referenced_column_id] = pc.[column_id]
                    WHERE       schemas.[name] = '{schemaName}'
                    FOR JSON PATH";

                return dbJsonReader.GetList<Models.Source.ForeignKeys.ForeignKey>(sqlQuery);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Unable to get schema [{schemaName}] foreign keys. " +
                     "See inner exception for details.", ex);
            }
        }

        public Task<IList<Models.Source.PrimaryKeys.Table>> GetPrimaryKeys(string schemaName)
        {
            ArgumentNullException.ThrowIfNull(schemaName, nameof(schemaName));

            try
            {
                logger.LogDebug($"Getting [{schemaName}] primary keys...");

                var sqlQuery = $@"
                    SELECT      tables.[name] as table_name,
                                columns.[name] as column_name
                    FROM        sys.tables tables
                    INNER JOIN  sys.schemas schemas
                                ON  tables.[schema_id] = schemas.[schema_id]
                    INNER JOIN  sys.indexes indexes
                                ON  tables.[object_id] = indexes.[object_id]
                    INNER JOIN  sys.index_columns index_columns
                                ON  indexes.[object_id] = index_columns.[object_id]
                                AND indexes.[index_id] = index_columns.[index_id]
                    INNER JOIN  sys.columns columns
                                ON  index_columns.[object_id] = columns.[object_id]
                                AND columns.[column_id] = index_columns.[column_id]
                    WHERE       schemas.[name] = '{schemaName}'
                    AND         indexes.[is_primary_key] = 1
                    FOR JSON AUTO";

                return dbJsonReader.GetList<Models.Source.PrimaryKeys.Table>(sqlQuery);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Unable to get schema [{schemaName}] primary keys. " +
                     "See inner exception for details.", ex);
            }
        }

        public Task<IList<Models.Source.TableStructure.Table>> GetTables(string schemaName)
        {
            ArgumentNullException.ThrowIfNull(schemaName);

            try
            {
                logger.LogDebug($"Getting schema [{schemaName}] tables...");

                var sqlQuery = $@"
                    SELECT      tables.[name] as table_name,
                                tables.[object_id] as table_id,
                                columns.[name] as column_name,
                                columns.[column_id] as column_id,
                                system_types.[name] as column_data_type
                    FROM        sys.tables tables
                    INNER JOIN  sys.schemas schemas
                                ON  tables.[schema_id] = schemas.[schema_id]
                    INNER JOIN  sys.columns columns
                                ON  tables.[object_id] = columns.[object_id]
                    INNER JOIN  sys.types user_types
                                ON  columns.[user_type_id] = user_types.[user_type_id]
                    INNER JOIN  sys.types system_types
                                ON  user_types.[system_type_id] = system_types.[user_type_id]
                    WHERE       schemas.[name] = '{schemaName}'
                    AND         tables.[type] = 'U'
                    AND         columns.[name] != 'rowguid'
                    FOR JSON AUTO";

                return dbJsonReader.GetList<Models.Source.TableStructure.Table>(sqlQuery);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Unable to get schema [{schemaName}] tables. " +
                     "See inner exception for details.", ex);
            }
        }
    }
}
