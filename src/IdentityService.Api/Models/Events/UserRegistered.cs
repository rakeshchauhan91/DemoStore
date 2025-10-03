using Infrastructure.Defaults;

namespace IdentityService.Api.Models.Events
{
    public class UserRegistered : IntegrationEvent
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }

        public UserRegistered(string userId, string email, string firstName)
        {
            UserId = userId;
            Email = email;
            FirstName = firstName;
        }
    }




    public class UserLoggedInEvent : IntegrationEvent
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public DateTime LoginAt { get; set; }
        public UserLoggedInEvent(string userId, string email )
        {
            UserId = userId;
            Email = email;
            LoginAt =   DateTime.Now;
        }
    }

    public class PasswordChangedEvent : IntegrationEvent
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public DateTime ChangedAt { get; set; }
        public PasswordChangedEvent(string userId, string email )
        {
            UserId = userId;
            Email = email;
            ChangedAt =  DateTime.Now;
        }
    }
}
