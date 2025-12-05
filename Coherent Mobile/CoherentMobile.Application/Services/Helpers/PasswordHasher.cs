using System.Security.Cryptography;
using System.Text;

namespace CoherentMobile.Application.Services.Helpers;

/// <summary>
/// Password hashing service using PBKDF2
/// </summary>
public class PasswordHasher
{
    private const int SaltSize = 32;
    private const int HashSize = 32;
    private const int Iterations = 100000;

    public static (string hash, string salt) HashPassword(string password)
    {
        // Generate salt
        var saltBytes = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }
        
        var salt = Convert.ToBase64String(saltBytes);
        
        // Generate hash
        using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256))
        {
            var hashBytes = pbkdf2.GetBytes(HashSize);
            var hash = Convert.ToBase64String(hashBytes);
            
            return (hash, salt);
        }
    }

    public static bool VerifyPassword(string password, string hash, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        
        using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256))
        {
            var hashBytes = pbkdf2.GetBytes(HashSize);
            var computedHash = Convert.ToBase64String(hashBytes);
            
            return computedHash == hash;
        }
    }
}
