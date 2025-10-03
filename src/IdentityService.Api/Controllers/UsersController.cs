using Infrastructure.Defaults.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdentityService.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = "ApiScope")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UnitOfWork<AppIdentityDbContext> _addressUOW;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            UnitOfWork<AppIdentityDbContext> addressRepository)
        {
            _userManager = userManager;
            _addressUOW = addressRepository;
        }

        private string GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // GET /api/users/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetCurrentUserId();
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            //var profileDto = new Models.DTO.UserProfileDto
            //    user.Email!, user.FirstName, user.LastName, user.PhoneNumber, user.EmailConfirmed, roles.ToList()
            //);

            return Ok();
        }

        // PUT /api/users/profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileRequest request)
        {
            var userId = GetCurrentUserId();
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) return NotFound();

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;
            // Password change handled by a separate endpoint (e.g., /api/auth/change-password)

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return NoContent();
        }

        // GET /api/users/addresses
        [HttpGet("addresses")]
        public async Task<IActionResult> GetAddresses()
        {
            var userId = GetCurrentUserId();
            var addresses = await _addressUOW.GetRepository<UserAddress,Guid>().FindAsync(a => a.UserId.ToString() == userId);

            return Ok(addresses.Select(AddressMapper.ToDto));
        }

        // POST /api/users/addresses
        [HttpPost("addresses")]
        public async Task<IActionResult> AddAddress([FromBody] AddressDto dto)
        {
            var userId = GetCurrentUserId();
            var newAddress = AddressMapper.ToEntity(dto, Guid.Parse(userId));

            await _addressUOW.GetRepository<UserAddress, Guid>().AddAsync(newAddress);
            await _addressUOW.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAddresses), new { id = newAddress.Id }, AddressMapper.ToDto(newAddress));
        }

        // PUT /api/users/addresses/{id}
        [HttpPut("addresses/{id}")]
        public async Task<IActionResult> UpdateAddress(Guid id, [FromBody] AddressDto dto)
        {
            var userId = GetCurrentUserId();
            var existingAddress = await _addressUOW.GetRepository<UserAddress, Guid>().GetByIdAsync(id);

            if (existingAddress == null || existingAddress.User.Id.ToString() != userId)
                return NotFound();

            // Manual mapping for update
            existingAddress.AddressLine1 = dto.AddressLine1;
            existingAddress.AddressLine2 = dto.AddressLine2;
            existingAddress.City = dto.City;
            existingAddress.State = dto.State;
            existingAddress.PostalCode = dto.PostalCode;
            existingAddress.Country = dto.Country;
            existingAddress.IsDefault = dto.IsDefault;

            await _addressUOW.GetRepository<UserAddress, Guid>().UpdateAsync(existingAddress);
            await _addressUOW.SaveChangesAsync();

            return NoContent();
        }

        // DELETE /api/users/addresses/{id}
        [HttpDelete("addresses/{id}")]
        public async Task<IActionResult> DeleteAddress(string  id)
        {
            var userId = GetCurrentUserId();
            var UserGuid = Guid.Parse(id);
            var existingAddress = await _addressUOW.GetRepository<UserAddress, Guid>().GetByIdAsync(UserGuid);

            if (existingAddress == null || existingAddress.User.Id != UserGuid)
                return NotFound();

            await _addressUOW.GetRepository<UserAddress, Guid>().DeleteAsync(existingAddress);
            await _addressUOW.SaveChangesAsync();

            return NoContent();
        }
    }
}
