using MediatR;
using CMS.Application.DTOs;

namespace CMS.Application.Features.Sites.Queries;

public class GetAllSitesQuery : IRequest<IEnumerable<SiteDto>>
{
}
