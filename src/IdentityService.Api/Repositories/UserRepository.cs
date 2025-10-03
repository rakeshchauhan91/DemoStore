using Infrastructure.Defaults.Persistence;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Api.Repositories
{
    public class UserRepository : GenericRepository<ApplicationUser, Guid, AppIdentityDbContext>, IUserRepository
    {
        public UserRepository(AppIdentityDbContext context) : base(context)
        {
        }

        public async Task<ApplicationUser?> GetUserWithRolesAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(u => u.UserRoles)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }

        public async Task<ApplicationUser?> GetUserWithAddressesAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(u => u.Addresses)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }

        public async Task<ApplicationUser?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }
    }

}
