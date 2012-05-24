using System;

namespace Reactivity.Objects
{
    [Serializable]
    public enum DeviceStatus { Unknown, Offline, Online, Paused, Error };

    /// <summary>
    /// A Device represents a type of hardware
    /// </summary>
    [Serializable]
    public class Device : ICloneable
    {
        /// <summary>
        /// GUID for the Device
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Device name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Device description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Type for the Device
        /// </summary>
        public Guid Type { get; set; }

        /// <summary>
        /// Device profile
        /// </summary>
        public string Profile { get; set; }

        /// <summary>
        /// Device configuration
        /// </summary>
        public string Configuration { get; set; }

        /// <summary>
        /// Current status of the device
        /// </summary> 
        public DeviceStatus Status { get; set; }

        public object Clone()
        {
            return new Device { Configuration = this.Configuration, Guid = this.Guid, Description = this.Description, Name = this.Name, Status = this.Status, Type = this.Type, Profile = this.Profile };
        }
    }
}
