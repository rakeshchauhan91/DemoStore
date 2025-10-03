using Infrastructure.Defaults.Persistence;
 
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Api.Repositories
{
    public class UserAddressRepository : GenericRepository<UserAddress, Guid, AppIdentityDbContext>, IUserAddressRepository
    {
        public UserAddressRepository(AppIdentityDbContext context) : base(context)
        {
        }

        public async Task<List<UserAddress>> GetUserAddressesAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(ua => ua.UserId == userId && !ua.IsDeleted)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<UserAddress?> GetUserAddressByIdAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.Id == addressId && !ua.IsDeleted, cancellationToken);
        }
    }
}
