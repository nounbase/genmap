namespace Nounbase.Services.Constants
{
    public static class NounbaseEnv
    {
        public const string BlobStorageConnectionString = "NOUNBASE_ENV_BLOB_STORAGE_CONNECTION_STRING";
        public const string ModelStorageContainerName = "NOUNBASE_ENV_MODEL_STORAGE_CONTAINER_NAME";

        public const string OpenAiApiKey = "NOUNBASE_ENV_OPENAI_API_KEY";
        public const string OpenAiThrottle = "NOUNBASE_ENV_OPENAI_THROTTLE";

        public const string SqlThrottle = "NOUNBASE_ENV_SQL_THROTTLE";

        public const string DefaultModelName = "NOUNBASE_ENV_DEFAULT_MODEL_NAME";
        public const string DiscoveryModelName = "NOUNBASE_ENV_DISCOVERY_MODEL_NAME";
        public const string NarrationModelName = "NOUNBASE_ENV_NARRATION_MODEL_NAME";
        public const string EnrichmentModelName = "NOUNBASE_ENV_ENRICHMENT_MODEL_NAME";

        public const string Temperature = "NOUNBASE_ENV_TEMPERATURE";
    }
}
