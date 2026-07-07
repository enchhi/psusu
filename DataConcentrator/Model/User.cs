using System.ComponentModel.DataAnnotations;

namespace DataConcentrator
{
    // F5: korisnik za login. Lozinka se cuva kao HASH (ne plaintext).
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public Role Role { get; set; }
    }
}
