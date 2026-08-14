using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Models
{
    public static class PasswordService
    {
        private static readonly PasswordHasher<object> Hasher = new();

        public static string Hash(string password)
        {
            return Hasher.HashPassword(null!, password);
        }

        public static bool Verify(string hash, string password)
        {
            var result = Hasher.VerifyHashedPassword(
                null!,
                hash,
                password
            );

            return result == PasswordVerificationResult.Success ||
                   result == PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}