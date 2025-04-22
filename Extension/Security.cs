using System.Security.Cryptography;
using Murmur;
using System.Text;

namespace HTTL_May_Xay_Dung.Extension
{
    public static class Security
    {
        private const int SaltSize = 16; // 128 bit
        private const int KeySize = 32;  // 256 bit
        private const int Iterations = 10000; // Higher values are slower but more secure
        private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

        // Method to hash a password with a unique salt
        public static string Hash(this string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            // Generate a random salt
            var salt = new byte[SaltSize];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            // Hash the password with the salt using PBKDF2
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithm, KeySize);

            // Return the salt and hash in Base64 format
            return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        // Method to verify a password against a hash
        public static bool Verify(this string unhashString, string hashedString)
        {
            if (string.IsNullOrEmpty(unhashString) || string.IsNullOrEmpty(hashedString))
                throw new ArgumentNullException(nameof(unhashString));

            var parts = hashedString.Split('.');
            if (parts.Length != 2)
                throw new FormatException("Invalid hashed password format.");

            var salt = Convert.FromBase64String(parts[0]);
            var hash = Convert.FromBase64String(parts[1]);

            // Hash the password with the same salt
            var hashToCompare = Rfc2898DeriveBytes.Pbkdf2(unhashString, salt, Iterations, HashAlgorithm, KeySize);

            // Compare the hash with the stored hash
            return CryptographicOperations.FixedTimeEquals(hash, hashToCompare);
        }

        public static int MurmurHash32(this string unhashString)
        {
            unhashString = unhashString.ToLower();
            var murmur = MurmurHash.Create32();
            var bytes = Encoding.UTF8.GetBytes(unhashString);
            return BitConverter.ToInt32(murmur.ComputeHash(bytes), 0);
        }

        public static int FNV1a32(this string input)
        {
            input = input.ToLower();
            const int fnvPrime = 0x01000193; // 32-bit FNV prime
            const int offsetBasis = unchecked((int)2166136261); // 32-bit offset basis

            int hash = offsetBasis;
            foreach (var c in input)
            {
                hash ^= c;
                hash *= fnvPrime;
            }
            return hash;
        }
    }
}
