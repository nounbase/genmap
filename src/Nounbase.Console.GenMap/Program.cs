using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nounbase.Services.Extensions;
using Nounbase.Services.SqlServer.Extensions;
using System.Text;
using static System.Environment;

namespace Nounbase.Console.GenMap
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var serviceProvider = CreateServiceProvider();
            var log = serviceProvider.GetRequiredService<ILogger<Program>>();

            if (ValidateEnvironmentVariables(log))
            {
                var mapFactory = serviceProvider.GetRequiredService<MapFactory>();
                var map = await mapFactory.CreateMap(MapConfiguration.FromEnvironmentVariables());

                System.Console.OutputEncoding = Encoding.ASCII;
                System.Console.Write(JsonConvert.SerializeObject(map, Formatting.Indented));
            }
            else
            {
                Exit(1); // Nope.
            }
        }

        private static IServiceProvider CreateServiceProvider() => new ServiceCollection()
            .AddDefaultServices()
            .AddSqlServerServices()
            .AddSingleton<MapFactory>()
            .AddLogging(s => s.AddSimpleConsole().SetMinimumLevel(
                GetEnvironmentVariable(NounbaseEnv.DoLog) is not null
                ? LogLevel.Information
                : LogLevel.Error))
            .BuildServiceProvider();


        private static bool ValidateEnvironmentVariables(ILogger log)
        {
            var valid = true;

            var envSchemaName = GetEnvironmentVariable(NounbaseEnv.SourceSchema);
            var envOpenAiApiKey = GetEnvironmentVariable(Services.Constants.NounbaseEnv.OpenAiApiKey);
            var envSqlConnectionString = GetEnvironmentVariable(Services.SqlServer.Constants.NounbaseEnv.DbConnectionString);

            if (string.IsNullOrWhiteSpace(envSchemaName))
            {
                log.LogError($"[{NounbaseEnv.SourceSchema}] not set.");
                valid = false;
            }

            if (string.IsNullOrWhiteSpace(envOpenAiApiKey))
            {
                log.LogError($"[{Services.Constants.NounbaseEnv.OpenAiApiKey}] not set.");
                valid = false;
            }

            if (string.IsNullOrWhiteSpace(envSqlConnectionString))
            {
                log.LogError($"[{Services.SqlServer.Constants.NounbaseEnv.DbConnectionString}] not set.");
                valid = false;
            }

            if (valid)
            {
                log.LogInformation($"{NewLine}Environment variables properly configured.{NewLine}");
            }
            else
            {
                log.LogError($"{NewLine}Environment variables not properly configured.{NewLine}");
            }

            return valid;
        }
    }
}
