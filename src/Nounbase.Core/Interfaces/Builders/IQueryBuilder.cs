using Nounbase.Core.Models;
using Nounbase.Core.Models.Schema;
using Nounbase.Core.Models.Semantic;
using Nounbase.Core.Models.Semantic.Relational;

namespace Nounbase.Core.Interfaces.Builders
{
    public interface IQueryBuilder
    {
        string BuildSampleQuery(Noun noun, Understanding understanding);
    }

    public interface IQueryBuilder<TSelector>
    {
        Query BuildRootDescriptionQuery(TSelector selector, Noun rootNoun, SemanticMap map);
        Query BuildBranchDescriptionQuery(TSelector selector, Noun rootNoun, SemanticMap map, Branch _1stDegBranch, Branch? _2ndDegBranch = null, int? top = null);
        Query BuildBranchLatestQuery(TSelector selector, Noun rootNoun, SemanticMap map, Branch _1stDegBranch, Branch? _2ndDegBranch = null, int last = 5);
        Query BuildBranchSummarizationQuery(TSelector selector, Noun rootNoun, SemanticMap map, Branch _1stDegBranch, Branch? _2ndDegBranch = null);
        IEnumerable<Query> BuildGroupSummarizationQueries(TSelector selector, Noun rootNoun, SemanticMap map, Branch _1stDegBranch, Branch? _2ndDegBranch = null);
    }
}
