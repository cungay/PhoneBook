using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReportService.Api.Core.Domain;
using ReportService.Api.Core.Infrastucture.Context;

namespace ReportService.Api.Core.Infrastucture.Mapping.Builders
{
    public class ReportRequestBuilder : IEntityTypeConfiguration<ReportRequest>
    {
        public void Configure(EntityTypeBuilder<ReportRequest> builder)
        {
            builder.ToTable("ReportRequest", ReportContext.DEFAULT_SCHEMA);

            builder.HasKey(ci => ci.Id);

            builder.Property(ci => ci.Id)
                .UseHiLo("report_request_hero")
                .IsRequired();

            builder.Property(p => p.RequestDate)
                .IsRequired();

            builder.Property(p => p.ReportStatus)
                .IsRequired();
        }
    }
}
