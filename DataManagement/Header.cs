using System.Runtime.InteropServices;

namespace DataManagement
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Header
    {
        /// <summary>
        /// New Guid for each file
        /// </summary>
        public Guid Guid;
        /// <summary>
        /// current datetime
        /// </summary>
        public DateTime Timestamp;
        /// <summary>
        /// HashSha256 of input file content
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] Checksum;
    }
}
