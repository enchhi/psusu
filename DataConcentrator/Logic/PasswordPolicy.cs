using System.Collections.Generic;
using System.Linq;

namespace DataConcentrator
{
    // F5: pravila lozinke - bar 15 karaktera, veliko/malo slovo i specijalni karakter.
    public static class PasswordPolicy
    {
        public const int MinLength = 15;

        public static List<string> Validate(string password)
        {
            var errors = new List<string>();
            var p = password ?? "";

            if (p.Length < MinLength)
                errors.Add("Lozinka mora imati bar " + MinLength + " karaktera.");
            if (!p.Any(char.IsUpper))
                errors.Add("Lozinka mora imati bar jedno veliko slovo.");
            if (!p.Any(char.IsLower))
                errors.Add("Lozinka mora imati bar jedno malo slovo.");
            if (!p.Any(IsSpecial))
                errors.Add("Lozinka mora imati bar jedan specijalni karakter.");

            return errors;
        }

        public static bool IsValid(string password) => Validate(password).Count == 0;

        private static bool IsSpecial(char c) => !char.IsLetterOrDigit(c);
    }
}
