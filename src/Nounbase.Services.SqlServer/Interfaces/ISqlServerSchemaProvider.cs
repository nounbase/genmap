namespace Nounbase.Services.SqlServer.Interfaces
{
    public interface ISqlServerSchemaProvider
    {
        Task<IList<Models.Source.TableStructure.Table>> GetTables(string schemaName);
        Task<IList<Models.Source.PrimaryKeys.Table>> GetPrimaryKeys(string schemaName);
        Task<IList<Models.Source.ForeignKeys.ForeignKey>> GetForeignKeys(string schemaName);
    }
}
