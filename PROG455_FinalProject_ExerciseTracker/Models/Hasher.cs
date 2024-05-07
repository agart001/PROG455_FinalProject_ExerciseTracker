using Newtonsoft.Json;
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
        public static string SHA256Hash(string input)
        {
            //Convert json into UTF8 bytes
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            //Built in SHA256 to compute a hash
            using (SHA256 sha256 = SHA256.Create())
            {
                //Compute the bytes
                byte[] hashedBytes = sha256.ComputeHash(inputBytes);

                //Use a string builder to append the bytes in the array
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashedBytes.Length; i++)
                {
                    builder.Append(hashedBytes[i].ToString("x2"));
                }

                //Return the string
                return builder.ToString();
            }
        }

        /// <summary>
        /// Encodes an object to a UTF-8 encoded Base64 string.
        /// </summary>
        /// <param name="input">The object to encode.</param>
        /// <returns>The UTF-8 encoded Base64 string.</returns>
        public static string UTF8Encode(object input)
        {
            // Serialize object to JSON
            string json = JsonConvert.SerializeObject(input, Formatting.None);

            // Convert JSON string to UTF-8 encoded byte array
            byte[] bytes = Encoding.UTF8.GetBytes(json);

            // Convert byte array to Base64 string
            string base64String = Convert.ToBase64String(bytes);

            return base64String;
        }

        /// <summary>
        /// Decodes a UTF-8 encoded Base64 string to the specified object type.
        /// </summary>
        /// <typeparam name="T">The type of object to decode.</typeparam>
        /// <param name="input">The UTF-8 encoded Base64 string.</param>
        /// <returns>The decoded object of type T.</returns>
        public static T UTF8Decode<T>(string input)
        {
            // Convert Base64 string to byte array
            byte[] bytes = Convert.FromBase64String(input);

            // Convert byte array to JSON string
            string json = Encoding.UTF8.GetString(bytes);

            // Deserialize JSON string to object of type T
            return JsonConvert.DeserializeObject<T>(json)!;
        }

        /// <summary>
        /// Creates a unique integer ID.
        /// </summary>
        /// <remarks>
        /// This method generates a unique integer ID by creating a numeric string of random digits.
        /// </remarks>
        /// <returns>A unique integer ID.</returns>
        public static int CreateID()
        {
            // Generate a new GUID
            int[] id = new int[9];
            Random rand = new Random();

            // Generate random digits
            for (int i = 0; i < id.Length; i++)
            {
                id[i] = rand.Next(1, 10);
            }

            // Parse the numeric string to an integer
            string str = string.Join("", id);
            int num = int.Parse(str);

            return num;
        }
    }

}
