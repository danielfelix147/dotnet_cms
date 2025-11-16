using MediatR;
using CMS.Application.DTOs;

namespace CMS.Application.Features.Content.Queries;

public class ExportSiteContentQuery : IRequest<SiteExportDto>
{
    public Guid SiteId { get; set; }
}
