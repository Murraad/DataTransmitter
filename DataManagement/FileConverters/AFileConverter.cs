using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace DataManagement.FileConverters
{
    /// <summary>
    /// Abstract converter for working with different HashAlgorithmes
    /// Each descendant will have different algiritm
    /// </summary>
    public abstract class AFileConverter : IDisposable
    {
        protected HashAlgorithm hashAlgorithm;

        protected abstract void InitializeAlgorithm();

        public AFileConverter() => this.InitializeAlgorithm();

        public bool AreChecksumsEqual(byte[] firstChecksum, byte[] secondChecksum) => new Span<byte>(firstChecksum).SequenceEqual(secondChecksum);

        public Header DeserializeHeaderFromFile(byte[] file)
        {
            var size = Marshal.SizeOf(typeof(Header));
            var ptr = Marshal.AllocHGlobal(size);
            try
            {
                if (size > file.Length)
                {
                    throw new ArgumentOutOfRangeException("Header size is longer then message body.");
                }

                Marshal.Copy(file, 0, ptr, size);
                var result = (Header)Marshal.PtrToStructure(ptr, typeof(Header));
                return result;
            }
            catch (Exception ex)
            {
                throw new FormatException();
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        public Header GenerateHeaderForFile(byte[] file) => new Header() { Guid = Guid.NewGuid(), Timestamp = DateTime.Now, Checksum = this.GetChecksum(file) };

        public byte[] GetBodyArrayFromFile(byte[] file)
        {
            var size = Marshal.SizeOf(typeof(Header));
            if (size > file.Length)
            {
                throw new FormatException("Header size is longer then message body.");
            }

            var result = file.Skip(size).ToArray();
            return result;
        }

        public byte[] GetChecksum(byte[] body) => hashAlgorithm.ComputeHash(body);

        public byte[] GetConvertedFile(byte[] header, byte[] body) => header.Concat(body).ToArray();

        public string GetFileName(Header header) => $"{header.Timestamp.ToString("yyyy-MM-dd HH-mm")} {header.Guid}.bin";

        public byte[] GetHeaderBytes<T>(T header) where T : struct
        {
            var size = Marshal.SizeOf(typeof(T));
            var array = new byte[size];
            var ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(header, ptr, true);
                Marshal.Copy(ptr, array, 0, size);
                return array;
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        public void Dispose() => this.hashAlgorithm.Dispose();
    }
}
