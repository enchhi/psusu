using System.Collections.Generic;
using System.Linq;

namespace DataConcentrator
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public bool Created { get; set; }             // true ako je korisnik novoregistrovan
        public List<string> Errors { get; } = new List<string>();
    }

    // F5: login ili registracija. Ako korisnik postoji -> proveri lozinku + ulogu.
    // Ako ne postoji -> registruj (uz pravila lozinke + jedinstvenost lozinke u bazi).
    public static class AuthService
    {
        public static AuthResult LoginOrRegister(string username, string password, Role role)
        {
            var result = new AuthResult();
            if (string.IsNullOrWhiteSpace(username))
            {
                result.Errors.Add("Unesite korisnicko ime.");
                return result;
            }

            var ctx = ContextClass.Instance;
            var hash = PasswordHasher.Hash(password);
            var existing = ctx.Users.FirstOrDefault(u => u.Username == username);

            if (existing != null)
            {
                // postojeci korisnik -> login
                if (existing.PasswordHash != hash)
                    result.Errors.Add("Pogresna lozinka.");
                else if (existing.Role != role)
                    result.Errors.Add("Pogresna uloga za ovog korisnika.");
                else
                    result.Success = true;

                if (result.Success)
                    Logger.Instance.Log(LogCategory.Login, "Login: " + username + " (" + role + ").");
                return result;
            }

            // novi korisnik -> registracija uz pravila
            result.Errors.AddRange(PasswordPolicy.Validate(password));
            if (ctx.Users.Any(u => u.PasswordHash == hash))
                result.Errors.Add("Ta lozinka vec postoji u bazi (mora biti jedinstvena).");

            if (result.Errors.Count > 0)
                return result;

            ctx.Users.Add(new User { Username = username, PasswordHash = hash, Role = role });
            ctx.SaveChanges();
            result.Success = true;
            result.Created = true;
            Logger.Instance.Log(LogCategory.Login, "Registrovan i ulogovan korisnik " + username + " (" + role + ").");
            return result;
        }
    }
}
