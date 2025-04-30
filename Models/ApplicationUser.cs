using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace triliza.Models
{
    public class ApplicationUser: IdentityUser
    {
        [NotMapped]
        public ClaimsIdentity? Username { get; internal set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public ICollection<Match>? UserMatches { get; set; }
        public int Gold { get; set; }
    }
}
