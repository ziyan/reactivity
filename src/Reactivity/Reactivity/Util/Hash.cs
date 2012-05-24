using System;

namespace Reactivity.Util
{
    /// <summary>
    /// Hash function
    /// </summary>
    public static class Hash
    {
        private static System.Security.Cryptography.SHA256 hash =
            new System.Security.Cryptography.SHA256CryptoServiceProvider();
        static Hash()
        {
            hash.Initialize();
        }
        /// <summary>
        /// Hash a message
        /// </summary>
        /// <param name="message">message in byte[]</param>
        /// <returns>hashed values in byte[]</returns>
        public static byte[] ToBytes(byte[] message)
        {
            return hash.ComputeHash(message);
        }
        /// <summary>
        /// Hash a message
        /// </summary>
        /// <param name="message">message in UTF-8 string</param>
        /// <returns>hashed values in byte[]</returns>
        public static byte[] ToBytes(string message)
        {
            return ToBytes(System.Text.Encoding.UTF8.GetBytes(message));
        }

        /// <summary>
        /// Hash a message
        /// </summary>
        /// <param name="message">message in UTF-8 string</param>
        /// <returns>hashed values in string</returns>
        public static string ToString(string message)
        {
            return BitConverter.ToString(ToBytes(message)).Replace("-", "").ToLower();
        }

        /// <summary>
        /// Hash a message
        /// </summary>
        /// <param name="message">message in byte[]</param>
        /// <returns>hashed values in string</returns>
        public static string ToString(byte[] message)
        {
            return BitConverter.ToString(ToBytes(message)).Replace("-", "").ToLower();
        }

        /// <summary>
        /// Hash message length for hex string
        /// </summary>
        public static int StringLength
        {
            get { return hash.HashSize / 4; }
        }

        /// <summary>
        /// Hash message length for binary
        /// </summary>
        public static int BytesLength
        {
            get { return hash.HashSize / 8; }
        }

        public static bool Test()
        {
            if (ToString("") != "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855") return false;
            if (ToString("abc") != "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad") return false;
            if (ToString("message digest") != "f7846f55cf23e14eebeab5b4e1550cad5b509e3348fbc4efa3a1413d393cb650") return false;
            if (ToString("secure hash algorithm") != "f30ceb2bb2829e79e4ca9753d35a8ecc00262d164cc077080295381cbd643f0d") return false;
            if (ToString("SHA256 is considered to be safe") != "6819d915c73f4d1e77e4e1b52d1fa0f9cf9beaead3939f15874bd988e2a23630") return false;
            if (ToString("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq") != "248d6a61d20638b8e5c026930c3e6039a33ce45964ff2167f6ecedd419db06c1") return false;
            if (ToString("For this sample, this 63-byte string will be used as input data") != "f08a78cbbaee082b052ae0708f32fa1e50c5c421aa772ba5dbb406a2ea6be342") return false;
            if (ToString("This is exactly 64 bytes long, not counting the terminating byte") != "ab64eff7e88e2e46165e29f2bce41826bd4c7b3552f6b382a9e7d3af47c245f8") return false;
            return true;
        }
    }
}
