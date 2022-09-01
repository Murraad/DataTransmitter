using System.Security.Cryptography;

namespace DataManagement.FileConverters
{
    /// <summary>
    /// File converter with SHA256 hashAlgorithm
    /// </summary>
    public class SHA256FileConverter : AFileConverter
    {
        public SHA256FileConverter()
        {
            this.hashAlgorithm = SHA256.Create();
        }
    }
}
