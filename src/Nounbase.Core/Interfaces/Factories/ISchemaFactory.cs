using Nounbase.Core.Models.Schema;

namespace Nounbase.Core.Interfaces.Factories
{
    public interface ISchemaFactory
    {
        string DatabaseType { get; }

        Task<Schema> CreateSchema(string schemaName);
    }
}
