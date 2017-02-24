using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace LowLevelDesign.Send2Procmon
{
    class NativeMethods
    {
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern SafeFileHandle CreateFile(string fileName, 
            [MarshalAs(UnmanagedType.U4)] FileAccess fileAccess, 
            [MarshalAs(UnmanagedType.U4)] FileShare fileShare, 
            IntPtr securityAttributes, 
            [MarshalAs(UnmanagedType.U4)] CreationDisposition creationDisposition, 
            [MarshalAs(UnmanagedType.U4)] FileAttributes flags, 
            IntPtr template);

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DeviceIoControl(SafeFileHandle hDevice, 
            uint IoControlCode, 
            [MarshalAs(UnmanagedType.AsAny)] [In] object InBuffer, 
            uint nInBufferSize, 
            [MarshalAs(UnmanagedType.AsAny)] [Out] object OutBuffer, 
            uint nOutBufferSize, 
            ref uint pBytesReturned, 
            [In] IntPtr Overlapped);

        [Flags]
        public enum FileAccess : uint
        {
            Read = 2147483648u,
            Write = 1073741824u,
            Execute = 536870912u,
            All = 268435456u
        }

        [Flags]
        public enum FileShare : uint
        {
            None = 0u,
            Read = 1u,
            Write = 2u,
            Delete = 4u
        }

        public enum CreationDisposition : uint
        {
            New = 1u,
            CreateAlways,
            OpenExisting,
            OpenAlways,
            TruncateExisting
        }

        [Flags]
        public enum FileAttributes : uint
        {
            Readonly = 1u,
            Hidden = 2u,
            System = 4u,
            Directory = 16u,
            Archive = 32u,
            Device = 64u,
            Normal = 128u,
            Temporary = 256u,
            SparseFile = 512u,
            ReparsePoint = 1024u,
            Compressed = 2048u,
            Offline = 4096u,
            NotContentIndexed = 8192u,
            Encrypted = 16384u,
            Write_Through = 2147483648u,
            Overlapped = 1073741824u,
            NoBuffering = 536870912u,
            RandomAccess = 268435456u,
            SequentialScan = 134217728u,
            DeleteOnClose = 67108864u,
            BackupSemantics = 33554432u,
            PosixSemantics = 16777216u,
            OpenReparsePoint = 2097152u,
            OpenNoRecall = 1048576u,
            FirstPipeInstance = 524288u
        }
    }
}
