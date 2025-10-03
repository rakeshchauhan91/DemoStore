using Infrastructure.Defaults.Persistence;

namespace IdentityService.Api.Interfaces
{
    
    public interface IUserAddressRepository : IGenericRepository<UserAddress, Guid>
    {
        Task<List<UserAddress>> GetUserAddressesAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<UserAddress?> GetUserAddressByIdAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default);
    }
}
