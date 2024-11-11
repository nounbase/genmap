using static System.Environment;
using static Nounbase.Core.Utilities.Environment;

namespace Nounbase.Console.GenMap
{
    public class MapConfiguration
    {
        public string? OutputPath { get; set; }
        public string? SchemaName { get; set; }

        public static MapConfiguration FromEnvironmentVariables() =>
            new MapConfiguration
            {
                OutputPath = GetEnvironmentVariable(NounbaseEnv.MapOutputPath),
                SchemaName = GetRequiredEnvironmentVariable(NounbaseEnv.SourceSchema)
            };
    }
}
