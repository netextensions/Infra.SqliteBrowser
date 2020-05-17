using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetExtensions.Infra.Sqlite;

namespace NetExtensions.Infra.SqliteBrowser
{
    public static class SqlBrowserExtensions
    {
        public static IServiceCollection AddSqliteBrowser(this IServiceCollection services, IConfiguration configuration, string connectionSetting)
        {
            var sqlBrowserParams = new SqlBrowserParameters();
            configuration.GetSection(nameof(SqlBrowserParameters)).Bind(sqlBrowserParams);

            if (string.IsNullOrWhiteSpace(sqlBrowserParams.FullDbPath))
            {
                var (_, dbFilePath) = SqliteExtension.ConnectionStringBuilder(configuration.GetConnectionString(connectionSetting));
                sqlBrowserParams.FullDbPath = dbFilePath;
            }

            services.AddSingleton(c => sqlBrowserParams);
            services.AddHostedService<SqlBrowser>();
            return services;
        }
    }
}