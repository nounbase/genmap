using Nounbase.Core.Models;
using Nounbase.Core.Models.Schema;
using Nounbase.Core.Models.Semantic;

namespace Nounbase.Core.Interfaces.Factories
{
    public interface INounFactory
    {
        Task<Noun> CreateNoun(Table table, Understanding understanding);
    }
}
