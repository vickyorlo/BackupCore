using System.IO;

namespace BackupCore
{
    /// <summary>
    /// A class for dealing with hashes of files.
    /// </summary>
    static class HashTools
    {
        /// <summary>
        /// Takes a string path to a file, loads it and computes a hash of the contents.
        /// </summary>
        /// <param name="file">The string containing the path to the file to hash</param>
        /// <returns>A byte array containing the computed hash.</returns>
        public static byte[] HashFile(string file)
        {
            try
            {
                byte[] hash;
                using (FileStream fs = File.OpenRead(file))
                {
                    using (System.Security.Cryptography.HashAlgorithm alg = System.Security.Cryptography.MD5.Create())
                    {
                        hash = alg.ComputeHash(fs);
                    }
                }
                return hash;
            }
            catch (FileNotFoundException e)
            {
                throw e;
            }

        }

        /// <summary>
        /// Compares two hashes.
        /// </summary>
        /// <param name="hash1">One hash to compare</param>
        /// <param name="hash2">Another hash to compare</param>
        /// <returns>True if the hashes are equal, false otherwise.</returns>
        public static bool CompareHashes(byte[] hash1, byte[] hash2)
        {
            if (hash1.Length == hash2.Length) return false;
            for (int i = 0; i < hash1.Length; i++)
            {
                if (hash1[i] != hash2[i]) return false;
            }
            return true;
        }
    }

}
