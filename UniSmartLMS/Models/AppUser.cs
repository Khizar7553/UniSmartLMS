using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using UniSmartLMS.Models.SectionD_People;

namespace UniSmartLMS.Models
{
    public class AppUser : IdentityUser
    {
        public int? PartyId { get; set; }
        public string FullName { get; set; }
        public string UserType { get; set; } // Student, Faculty, Librarian, Admin
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        // ✅ ADD THIS ONE LINE
        public string PreferredLanguage { get; set; } = "English"; // English, Urdu, RomanUrdu

        [ForeignKey("PartyId")]
        public virtual Party Party { get; set; }
    }
}