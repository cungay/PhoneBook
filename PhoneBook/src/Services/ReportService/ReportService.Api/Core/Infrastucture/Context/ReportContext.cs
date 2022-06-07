using Microsoft.EntityFrameworkCore;
using ReportService.Api.Core.Domain;
using ReportService.Api.Core.Infrastucture.Mapping.Builders;

namespace ReportService.Api.Core.Infrastucture.Context
{
    public class ReportContext : DbContext
    {
        public const string DEFAULT_SCHEMA = "report";

        public ReportContext(DbContextOptions<ReportContext> options) : base(options)
        {
        }

        public DbSet<ReportRequest> ReportRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new ReportRequestBuilder());
        }
    }
}
