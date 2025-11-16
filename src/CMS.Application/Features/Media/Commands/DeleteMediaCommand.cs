using MediatR;

namespace CMS.Application.Features.Media.Commands;

public class DeleteMediaCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public string MediaType { get; set; } = "Image"; // Image or File
}
