using System.Security.Cryptography;
using System.Text;

namespace DataConcentrator
{
    // F5: hash lozinke (SHA-256). Ne cuvamo plaintext; jednakost hash-eva = jednake lozinke
    // (koristi se za pravilo da ne sme postojati ista lozinka u bazi).
    public static class PasswordHasher
    {
        public static string Hash(string password)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password ?? ""));
                var sb = new StringBuilder(bytes.Length * 2);
                foreach (var b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
