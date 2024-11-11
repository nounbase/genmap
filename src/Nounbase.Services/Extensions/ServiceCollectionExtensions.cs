using Microsoft.Extensions.DependencyInjection;
using Nounbase.Core.Interfaces.Clients;
using Nounbase.Core.Interfaces.Configuration;
using Nounbase.Core.Interfaces.Enrichers;
using Nounbase.Core.Interfaces.Factories;
using Nounbase.Core.Interfaces.Narrators;
using Nounbase.Services.Clients;
using Nounbase.Services.Configuration;
using Nounbase.Services.Enrichers;
using Nounbase.Services.Factories;
using Nounbase.Services.Narrators;

namespace Nounbase.Services.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultServices(this IServiceCollection services) =>
            (services ?? throw new ArgumentNullException(nameof(services)))
            .AddScoped<IChatGptClient, OpenAiChatGptClient>()
            .AddScoped<IModelConfiguration, EnvModelConfiguration>()
            .AddScoped<INarrator, Narrator>()
            .AddScoped<INounEnricher, NounEnricher>()
            .AddScoped<INounFactory, NounFactory>();
    }
}
