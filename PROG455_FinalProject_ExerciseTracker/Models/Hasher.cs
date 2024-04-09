using System.Security.Cryptography;
using System.Text;

namespace PROG455_FinalProject_ExerciseTracker.Models
{
    /// <summary>
    /// Provides methods for hashing strings.
    /// </summary>
    public static class Hasher
    {
        /// <summary>
        /// Computes the SHA-256 hash value of the input string.
        /// </summary>
        /// <param name="input">The string to be hashed.</param>
        /// <returns>The hexadecimal representation of the computed hash.</returns>
        public static string GetStringHash(string input)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(inputBytes);

                StringBuilder builder = new StringBuilder();

                for (int i = 0; i < hashedBytes.Length; i++)
                {
                    builder.Append(hashedBytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }

}
