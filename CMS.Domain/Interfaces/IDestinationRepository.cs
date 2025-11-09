using CMS.Domain.Entities;

namespace CMS.Domain.Interfaces;

public interface IDestinationRepository : IRepository<Destination>
{
    Task<Destination?> GetDestinationWithToursAsync(Guid destinationId);
    Task<IEnumerable<Destination>> GetDestinationsBySiteIdAsync(Guid siteId);
}
