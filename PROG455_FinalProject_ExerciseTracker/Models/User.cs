using static PROG455_FinalProject_ExerciseTracker.Models.Hasher;

namespace PROG455_FinalProject_ExerciseTracker.Models
{
    /// <summary>
    /// Represents a user entity.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Gets or sets the unique identifier of the user.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the password of the user.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class with default values.
        /// </summary>
        public User()
        {
            ID = 0;
            Name = string.Empty;
            Password = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class with the specified name and password.
        /// </summary>
        /// <param name="name">The name of the user.</param>
        /// <param name="password">The password of the user.</param>
        public User(string name, string password)
        {
            ID = CreateID();
            Name = name;
            Password = password;
        }

        /// <summary>
        /// Generates a unique identifier for the user.
        /// </summary>
        /// <returns>A unique identifier for the user.</returns>
        private int CreateID()
        {
            // Generate a new GUID
            int[] id = new int[9];
            Random rand;

            for (int i = 0; i < id.Length; i++)
            {
                rand = new Random();
                id[i] = rand.Next(1, 10);
            }

            // Parse the numeric string to an integer
            string str = string.Join("", id);
            int num = int.Parse(str);

            return num;
        }
    }
}
