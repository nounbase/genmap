using Microsoft.Extensions.DependencyInjection;
using Nounbase.Core.Interfaces.Builders;
using Nounbase.Core.Interfaces.Factories;
using Nounbase.Core.Interfaces.Readers;
using Nounbase.Core.Interfaces.Samplers;
using Nounbase.Services.SqlServer.Builders;
using Nounbase.Services.SqlServer.Factories;
using Nounbase.Services.SqlServer.Interfaces;
using Nounbase.Services.SqlServer.Providers;
using Nounbase.Services.SqlServer.Readers;
using Nounbase.Services.SqlServer.Samplers;

namespace Nounbase.Services.SqlServer.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSqlServerServices(this IServiceCollection services) =>
            (services ?? throw new ArgumentNullException(nameof(services)))
            .AddScoped<IDbJsonReader, SqlServerDbJsonReader>(sp => new SqlServerDbJsonReader()) // Default constructor automatically obtains config from env.
            .AddScoped<IDbRecordSetReader, SqlServerDbRecordSetReader>(sp => new SqlServerDbRecordSetReader()) // Default constructor automatically obtains config from env.
            .AddScoped<IQueryBuilder, SqlServerQueryBuilder>()
            .AddScoped<INounSampler, SqlServerNounSampler>()
            .AddScoped<ISchemaFactory, SqlServerSchemaFactory>()
            .AddScoped<ISqlServerSchemaProvider, SqlServerSchemaProvider>()
            .AddScoped<ITableSampler, SqlServerTableSampler>();
    }
}
 
