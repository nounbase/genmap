using Nounbase.Core.Models;
using Nounbase.Core.Models.Schema;

namespace Nounbase.Core.Interfaces.Samplers
{
    public interface ITableSampler
    {
        Task<DbRecordSet> GetTableSampleSet(Table table, Understanding understanding, int? sampleSize = null);
    }
}
