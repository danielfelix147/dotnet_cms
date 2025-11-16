using MediatR;
using CMS.Application.DTOs;

namespace CMS.Application.Features.Media.Queries;

public class GetMediaBySiteIdQuery : IRequest<IEnumerable<MediaDto>>
{
    public Guid SiteId { get; set; }
}
