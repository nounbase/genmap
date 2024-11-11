using Nounbase.Core.Models;
using Nounbase.Core.Models.Semantic;

namespace Nounbase.Core.Interfaces.Enrichers
{
    public interface INounEnricher
    {
        Task<Noun> Enrich(Noun noun, Understanding understanding);
    }
}
