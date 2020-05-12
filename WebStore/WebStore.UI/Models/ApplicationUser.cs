using Microsoft.AspNetCore.Identity;

namespace WebStore.UI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }

        public string City { get; set; }
    }
}
