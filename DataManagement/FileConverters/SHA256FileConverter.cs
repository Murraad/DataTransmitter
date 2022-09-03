using System.Security.Cryptography;

namespace DataManagement.FileConverters
{
    /// <summary>
    /// File converter with SHA256 hashAlgorithm
    /// </summary>
    public class SHA256FileConverter : AFileConverter
    {
        protected override void InitializeAlgorithm() => this.hashAlgorithm = SHA256.Create();
    }
}
