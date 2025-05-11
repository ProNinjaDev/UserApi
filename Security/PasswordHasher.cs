using System.Security.Cryptography;

namespace UserApi.Security {
    public static class PasswordHasher {
        private const int SaltSize = 16; // 128 бит
        private const int HashSize = 32; // 256 бит
        private const int NumIterations = 10000;
        private const string NameAlgorithm = "PBKDF2_SHA256";

        public struct HashedPasswordComponents
        {
            public byte[] Hash { get; init; }
            public byte[] Salt { get; init; }
            public int Iterations { get; init; }
            public string Algorithm { get; init; }
        }

        public static HashedPasswordComponents GenerateHashedPassword(string password) {
            byte[] salt = new byte[SaltSize];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            rng.Dispose();

            var keyDeriver = new Rfc2898DeriveBytes(password, salt, NumIterations, HashAlgorithmName.SHA256);
            byte[] hash = keyDeriver.GetBytes(HashSize);

            return new HashedPasswordComponents {
                Hash = hash,
                Salt = salt,
                Iterations = NumIterations,
                Algorithm = NameAlgorithm
            };
        }

        public static bool IsMatchPasswords(string password, byte[] storedHash, byte[] storedSalt, int storedIterations, string storedAlgorithm) {
            if (string.IsNullOrWhiteSpace(password))
                return false;
            if (storedHash == null || storedSalt == null)
                return false;
            if (storedHash.Length != HashSize)
                return false;
            if(storedSalt.Length != SaltSize)
                return false;
            if(storedAlgorithm != NameAlgorithm)
                return false;
            
            var keyDeriverVerify = new Rfc2898DeriveBytes(password, storedSalt, storedIterations, HashAlgorithmName.SHA256);
            byte[] hashVerify = keyDeriverVerify.GetBytes(storedHash.Length);

            return CryptographicOperations.FixedTimeEquals(hashVerify, storedHash);
        }
    }
}