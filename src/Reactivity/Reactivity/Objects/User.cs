using System;

namespace Reactivity.Objects
{
    /// <summary>
    /// Represents a User
    /// </summary>
    [Serializable]
    public class User : ICloneable
    {
        /// <summary>
        /// ID of the user
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Username for the user
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Name for the user
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the user
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Permission of the user
        /// </summary>
        public int Permission { get; set; }

        public static readonly int PERMISSION_ADMIN = 1 << 0;
        public static readonly int PERMISSION_SUBSCRIBE = 1 << 1;
        public static readonly int PERMISSION_CONTROL = 1 << 2;
        public static readonly int PERMISSION_STATS = 1 << 3;

        /// <summary>
        /// Validation
        /// </summary>
        public bool IsValid
        {
            get
            {
                return ID > 0 &&
                Name != null &&
                Name.Length <= 50 &&
                Username != null &&
                Username.Length <= 50 &&
                Description != null &&
                Description.Length <= 255;
            }
        }

        public object Clone()
        {
            return new User { ID = this.ID, Username = this.Username, Name = this.Name, Description = this.Description, Permission = this.Permission };
        }
    }
}
