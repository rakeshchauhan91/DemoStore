using Infrastructure.Defaults.Persistence;

namespace IdentityService.Api.Services
{

    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uow;
        private readonly IUserRepository _userRepository;
        private readonly IUserAddressRepository _addressRepository;

        public UserService(
            IUnitOfWork uow,
            IUserRepository userRepository,
            IUserAddressRepository addressRepository)
        {
            _uow = uow;
            _userRepository = userRepository;
            _addressRepository = addressRepository;
        }

        public async Task<UserProfileDto> GetUserProfileAsync(Guid userId)
        {
            var user = await _userRepository.GetUserWithRolesAsync(userId);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            return new UserProfileDto
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                IsEmailVerified = user.IsEmailVerified,
                Roles = user.UserRoles.Select(r => r.Role).ToList()
            };
        }

        public async Task UpdateUserProfileAsync(Guid userId, UpdateUserProfileRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            user.FirstName = request.FirstName ?? user.FirstName;
            user.LastName = request.LastName ?? user.LastName;
            user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;

            await _userRepository.UpdateAsync(user); // This will set UpdatedAt
            await _uow.SaveChangesAsync();
        }

        public async Task<List<AddressDto>> GetUserAddressesAsync(Guid userId)
        {
            var addresses = await _addressRepository.GetUserAddressesAsync(userId);

            return addresses.Select(ua => new AddressDto(
                ua.Id,
                ua.AddressLine1,
                ua.AddressLine2,
                ua.City,
                ua.State,
                ua.PostalCode,
                ua.Country,
                ua.IsDefault
            )).ToList();
        }

        public async Task<AddressDto> AddUserAddressAsync(Guid userId, CreateAddressRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            var newAddress = new UserAddress
            {
                UserId = userId,
                AddressLine1 = request.AddressLine1,
                AddressLine2 = request.AddressLine2,
                City = request.City,
                State = request.State,
                PostalCode = request.PostalCode,
                Country = request.Country,
                IsDefault = request.IsDefault
            };

            await _addressRepository.AddAsync(newAddress);
            await _uow.SaveChangesAsync();

            return new AddressDto(
                newAddress.Id,
                newAddress.AddressLine1,
                newAddress.AddressLine2,
                newAddress.City,
                newAddress.State,
                newAddress.PostalCode,
                newAddress.Country,
                newAddress.IsDefault
            );
        }

        public async Task UpdateUserAddressAsync(Guid userId, Guid addressId, UpdateAddressRequest request)
        {
            var address = await _addressRepository.GetUserAddressByIdAsync(userId, addressId);

            if (address == null)
            {
                throw new KeyNotFoundException("Address not found or does not belong to user.");
            }

            address.AddressLine1 = request.AddressLine1 ?? address.AddressLine1;
            address.AddressLine2 = request.AddressLine2 ?? address.AddressLine2;
            address.City = request.City ?? address.City;
            address.State = request.State ?? address.State;
            address.PostalCode = request.PostalCode ?? address.PostalCode;
            address.Country = request.Country ?? address.Country;
            address.IsDefault = request.IsDefault;

            await _addressRepository.UpdateAsync(address);
            await _uow.SaveChangesAsync();
        }

        public async Task DeleteUserAddressAsync(Guid userId, Guid addressId)
        {
            var address = await _addressRepository.GetUserAddressByIdAsync(userId, addressId);

            if (address == null)
            {
                throw new KeyNotFoundException("Address not found or does not belong to user.");
            }

          //  await _addressRepository.SoftDeleteAsync(addressId);
            await _uow.SaveChangesAsync();
        }
    }
}
