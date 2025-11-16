using CMS.Application.DTOs;
using CMS.Domain.Entities;

namespace CMS.Application.Mappings;

public static class DestinationMappings
{
    public static DestinationDto ToDto(this Destination destination)
    {
        return new DestinationDto
        {
            Id = destination.Id,
            SiteId = destination.SiteId,
            DestinationId = destination.DestinationId,
            Destination = destination.Name,
            Description = destination.Description,
            IsPublished = destination.IsPublished
        };
    }

    public static List<DestinationDto> ToDto(this IEnumerable<Destination> destinations)
    {
        return destinations.Select(d => d.ToDto()).ToList();
    }
}
