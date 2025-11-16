using CMS.Application.DTOs;
using CMS.Domain.Entities;

namespace CMS.Application.Mappings;

public static class TourMappings
{
    public static TourDto ToDto(this Tour tour)
    {
        return new TourDto
        {
            Id = tour.Id,
            DestinationId = tour.DestinationId,
            TourId = tour.TourId,
            Name = tour.Name,
            Description = tour.Description,
            Price = tour.Price,
            IsPublished = tour.IsPublished
        };
    }

    public static List<TourDto> ToDto(this IEnumerable<Tour> tours)
    {
        return tours.Select(t => t.ToDto()).ToList();
    }
}
