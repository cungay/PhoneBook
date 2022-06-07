using Microsoft.EntityFrameworkCore;
using ReportService.Api.Core.Infrastucture.Context;
using System.Reflection;

namespace ReportService.Api.Core.Infrastucture.Startup
{
    public class DbStartup : IStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddEntityFrameworkSqlServer()
                .AddDbContext<ReportContext>(options =>
                {
                    options.UseSqlServer(configuration["ConnectionString"],
                        sqlServerOptionsAction: sqlOptions =>
                        {
                            sqlOptions.MigrationsAssembly(typeof(Program).GetTypeInfo().Assembly.GetName().Name);
                            sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                        });
                });
        }

        public void Configure(IApplicationBuilder application)
        {
            
        }
    }
}
