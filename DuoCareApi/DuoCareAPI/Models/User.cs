using Microsoft.AspNetCore.Identity;

namespace DuoCareAPI.Models
{
    public class User : IdentityUser
    {
        public string FullName { get; set; }

        public string ProfilePhotoBase64 { get; set; } = "";
    }
}
