namespace IdentityService.Api.Models.DTO
{
    public record UserProfileDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsEmailVerified { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
       
    }
    public static class UserProfileMapper
    {
        public static AddressDto ToDto(UserAddress entity) => new(
            entity.Id, entity.AddressLine1, entity.AddressLine2, entity.City, entity.State, entity.PostalCode, entity.Country, entity.IsDefault);

        public static UserAddress ToEntity(AddressDto dto, Guid userId, Guid? id = null) => new()
        {
            Id = id ?? dto.Id ?? Guid.Empty, // Use the provided ID or 0 for new
            UserId = userId,
            AddressLine1 = dto.AddressLine1,
            AddressLine2 = dto.AddressLine2,
            City = dto.City,
            State = dto.State,
            PostalCode = dto.PostalCode,
            Country = dto.Country,
            IsDefault = dto.IsDefault
        };
    }
}
