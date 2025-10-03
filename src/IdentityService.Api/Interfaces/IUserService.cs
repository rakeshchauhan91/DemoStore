using IdentityService.Api.Models.DTO;

namespace IdentityService.Api.Interfaces
{

    public interface IUserService
    {
        Task<UserProfileDto> GetUserProfileAsync(Guid userId);
        Task UpdateUserProfileAsync(Guid userId, UpdateUserProfileRequest request);
        Task<List<AddressDto>> GetUserAddressesAsync(Guid userId);
        Task<AddressDto> AddUserAddressAsync(Guid userId, CreateAddressRequest request);
        Task UpdateUserAddressAsync(Guid userId, Guid addressId, UpdateAddressRequest request);
        Task DeleteUserAddressAsync(Guid userId, Guid addressId);
    }


}
