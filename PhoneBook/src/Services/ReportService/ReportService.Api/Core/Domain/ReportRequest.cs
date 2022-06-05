namespace ReportService.Api.Core.Domain
{
    public class ReportRequest
    {
        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the request date
        /// </summary>
        public DateTime RequestDate { get; set; }

        /// <summary>
        /// Gets or sets the report status identifier
        /// </summary>
        public int ReportStatusId { get; set; }

        /// <summary>
        /// Gets or sets the report status
        /// </summary>
        public ReportStatus ReportStatus
        {
            get => (ReportStatus)ReportStatusId;
            set => ReportStatusId = (int)value;
        }
    }
}
