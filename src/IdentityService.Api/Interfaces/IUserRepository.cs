using Infrastructure.Defaults.Persistence;

namespace IdentityService.Api.Interfaces
{
    public interface IUserRepository : IGenericRepository<ApplicationUser, Guid>
    {
        Task<ApplicationUser?> GetUserWithRolesAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<ApplicationUser?> GetUserWithAddressesAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<ApplicationUser?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    }
   
}
