using Nounbase.Core.Models;
using Nounbase.Core.Models.Schema;
using Nounbase.Core.Models.Semantic;

namespace Nounbase.Core.Interfaces.Samplers
{
    public interface INounSampler
    {
        Task<DbRecordSet?> GetNounSampleSet(Noun noun, Understanding understanding);
    }
}
