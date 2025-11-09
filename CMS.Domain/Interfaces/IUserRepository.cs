using CMS.Domain.Entities;

namespace CMS.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetUsersBySiteIdAsync(Guid siteId);
}
