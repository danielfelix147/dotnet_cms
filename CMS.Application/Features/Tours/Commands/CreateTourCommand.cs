using MediatR;
using CMS.Application.DTOs;

namespace CMS.Application.Features.Tours.Commands;

public class CreateTourCommand : IRequest<TourDto>
{
    public Guid DestinationId { get; set; }
    public string TourId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsPublished { get; set; }
}
