using Nounbase.Core.Models.Semantic;

namespace Nounbase.Core.Interfaces.Models
{
    public interface INoun
    {
        public string? TableName { get; set; }

        public List<Dimension> Dimensions { get; set; }
        public List<Property> Properties { get; set; }
    }
}
