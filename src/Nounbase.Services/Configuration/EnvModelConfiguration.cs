using Nounbase.Core.Interfaces.Configuration;
using Nounbase.Services.Constants;
using static System.Environment;

namespace Nounbase.Services.Configuration
{
    public class EnvModelConfiguration : IModelConfiguration
    {
        public EnvModelConfiguration() => Configure();

        public string DefaultModelName { get; private set; }
        public string DiscoveryModelName { get; private set; }
        public string EnrichmentModelName { get; private set; }
        public string NarrationModelName { get; private set; }

        private void Configure()
        {
            var env_defaultModelName = GetEnvironmentVariable(NounbaseEnv.DefaultModelName);
            var env_discoveryModelName = GetEnvironmentVariable(NounbaseEnv.DiscoveryModelName);
            var env_enrichmentModelName = GetEnvironmentVariable(NounbaseEnv.EnrichmentModelName);
            var env_narrationModelName = GetEnvironmentVariable(NounbaseEnv.NarrationModelName);

            DefaultModelName = env_defaultModelName ?? "gpt-4o"; // Default to GPT-4o OpenAI model?
            DiscoveryModelName = env_discoveryModelName ?? DefaultModelName!;
            EnrichmentModelName = env_enrichmentModelName ?? DefaultModelName!;
            NarrationModelName = env_narrationModelName ?? DefaultModelName!;
        }
    }
}
