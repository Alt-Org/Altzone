using System;
using System.IO;
using System.Text;
#if PRG_CRYPTO_FILES
using System.Security.Cryptography;
#endif

namespace Prg.Util
{
    /// <summary>
    /// Collection of file related utilities.<br />
    /// Ideas form: Encryption And Decryption Using A Symmetric Key In C#<br />
    /// https://www.c-sharpcorner.com/article/encryption-and-decryption-using-a-symmetric-key-in-c-sharp/
    /// </summary>
    /// <remarks>
    /// For file read operation we use flags: <c>FileAccess.Read</c> and <c>FileShare.ReadWrite</c>.
    /// </remarks>
    public static class FileUtil
    {
        public static readonly Encoding Encoding = new UTF8Encoding(false, false);

        public static void WriteAllText(string path, string contents)
        {
            File.WriteAllText(path, contents, Encoding);
        }

        public static string ReadAllText(string path)
        {
            // Uses StreamReader to read the file in FileShare.ReadWrite mode to allow file being edited elsewhere during read.
            return InternalReadAllText(path, Encoding);
        }

        public static string[] ReadAllLines(string path)
        {
            // Uses StreamReader to read the file in FileShare.ReadWrite mode to allow file being edited elsewhere during read.
            return InternalReadAllLines(path, Encoding);
        }

#if PRG_CRYPTO_FILES
        // https://simple.wikipedia.org/wiki/Largest_known_prime_number
        private static readonly byte[] _wikiNumber1 = Encoding.ASCII.GetBytes("170141183460469231731687303715884105727");

        private static readonly byte[] _wikiNumber2 = Encoding.ASCII.GetBytes("20988936657440586486151264256610222593863921"[..16]);

        private const string DataPrefix = "{SHA256}";

        public static void WriteAllText(string path, string contents, string key)
        {
            File.WriteAllText(path, WriteString(contents, key), Encoding);
        }

        public static string ReadAllText(string path, string key)
        {
            return ReadString(InternalReadAllText(path, Encoding), key);
        }

        public static string[] ReadAllLines(string path, string key)
        {
            // Try to follow semantics of // https://learn.microsoft.com/en-us/dotnet/api/system.io.file.readalllines
            // A line is defined as a sequence of characters followed by a carriage return ('\r'), a line feed ('\n'),
            // or a carriage return immediately followed by a line feed.
            // The resulting string does not contain the terminating carriage return and/or line feed.
            var terminators = new[]
            {
                "\r\n",
                "\r",
                "\n"
            };
            return ReadString(InternalReadAllText(path, Encoding), key).Split(terminators, StringSplitOptions.None);
        }

        /// <summary>
        /// Converts a plaint text string to encrypted and base64 encoded string with prefix.
        /// </summary>
        public static string WriteString(string plainText, string key)
        {
            using var algo = CreateAlgo(key);
            var encryptor = algo.CreateEncryptor(algo.Key, algo.IV);

            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            using (var streamWriter = new StreamWriter(cryptoStream))
            {
                streamWriter.Write(plainText);
            }
            var array = memoryStream.ToArray();
            return $"{DataPrefix}{Convert.ToBase64String(array)}";
        }

        /// <summary>
        /// Converts (decrypts) a prefixed, encrypted base64 encoded string to plain text string.
        /// </summary>
        public static string ReadString(string cipherText, string key)
        {
            // Note that SymmetricAlgorithm.Clear is called automatically on Dispose-
            using var algo = CreateAlgo(key);
            using var decryptor = algo.CreateDecryptor(algo.Key, algo.IV);

            var payload = new StringBuilder(cipherText)
                .Remove(0, DataPrefix.Length)
                .ToString();
            var buffer = Convert.FromBase64String(payload);
            using var memoryStream = new MemoryStream(buffer);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream);
            return streamReader.ReadToEnd();
        }

        private static SymmetricAlgorithm CreateAlgo(string key)
        {
            var aes = Aes.Create();
            var passwordBytes = new Rfc2898DeriveBytes(key, _wikiNumber1, 19, HashAlgorithmName.SHA256);
            aes.Key = passwordBytes.GetBytes(32);
            aes.IV = _wikiNumber2;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            return aes;
        }
#endif

        private static string[] InternalReadAllLines(string filePath, Encoding encoding)
        {
            // Try to follow semantics of // https://learn.microsoft.com/en-us/dotnet/api/system.io.file.readalllines
            // A line is defined as a sequence of characters followed by a carriage return ('\r'), a line feed ('\n'),
            // or a carriage return immediately followed by a line feed.
            // The resulting string does not contain the terminating carriage return and/or line feed.
            var terminators = new[]
            {
                "\r\n",
                "\r",
                "\n"
            };
            return InternalReadAllText(filePath, encoding).Split(terminators, StringSplitOptions.None);
        }

        private static string InternalReadAllText(string filePath, Encoding encoding)
        {
            // https://stackoverflow.com/questions/3545402/any-difference-between-file-readalltext-and-using-a-streamreader-to-read-file
            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var streamReader = new StreamReader(stream, encoding);
            return streamReader.ReadToEnd();
        }
    }
}
