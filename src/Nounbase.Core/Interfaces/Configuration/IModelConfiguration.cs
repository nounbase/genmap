namespace Nounbase.Core.Interfaces.Configuration
{
    public interface IModelConfiguration
    {
        public string DefaultModelName { get; }
        public string DiscoveryModelName { get; }
        public string EnrichmentModelName { get; }
        public string NarrationModelName { get; }
    }
}
