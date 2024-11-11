using Nounbase.Core.Models.Schema;

namespace Nounbase.Core.Interfaces.Readers
{
    public interface IDbRecordSetReader
    {
        Task<DbRecordSet> GetRecordSet(string sqlQuery);
    }
}
