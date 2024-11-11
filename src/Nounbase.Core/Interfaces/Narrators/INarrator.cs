using Nounbase.Core.Models;
using Nounbase.Core.Models.Semantic;
using Nounbase.Core.Models.Semantic.Narrative;

namespace Nounbase.Core.Interfaces.Narrators
{
    public interface INarrator
    {
        Task<ModelNarrative> Narrate(Understanding understanding);
    }
}
