using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LowLevelDesign.Send2Procmon
{
    public class ProcmonLogger : IDisposable
    {
        private const uint IoCtlCode = 2503311876;
        private const int MaxSingleMessageLength = 2047;

        private readonly SafeFileHandle hProcmonDevice;

        /// <summary>
        /// Creates the logger class.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Throws the exception when procmon is not tracing (logging device 
        /// is not available).
        /// </exception>
        public ProcmonLogger()
        {
            var hProcmonDevice = NativeMethods.CreateFile(
                @"\\.\Global\ProcmonDebugLogger", NativeMethods.FileAccess.Write | NativeMethods.FileAccess.Read,
                NativeMethods.FileShare.Write, IntPtr.Zero, NativeMethods.CreationDisposition.OpenExisting,
                NativeMethods.FileAttributes.Normal, IntPtr.Zero);
            if (hProcmonDevice.IsInvalid || hProcmonDevice.IsClosed) {
                int err = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"Can't connect to procmon device: 0x{err:X}. Probably procmon is not running.");
            }
        }

        /// <summary>
        /// Sends a messages to procmon.
        /// </summary>
        /// <param name="messages">A message to send</param>
        /// <exception cref="InvalidOperationException">
        /// Throws the exception when procmon is not tracing.
        /// </exception>
        public void SendToProcmon(string message)
        {
            SplitMessagesIfNecessaryAndSendThemToProcmon(message);
        }

        /// <summary>
        /// Sends a collection of messages to procmon.
        /// </summary>
        /// <param name="messages">A collection of messages to sent</param>
        /// <exception cref="InvalidOperationException">
        /// Throws the exception when procmon is not tracing.
        /// </exception>
        public void SendToProcmon(IEnumerable<string> messages)
        {
            foreach (var message in messages) {
                SplitMessagesIfNecessaryAndSendThemToProcmon(message);
            }
        }

        private void SplitMessagesIfNecessaryAndSendThemToProcmon(string message)
        {
            if (message.Length <= MaxSingleMessageLength) {
                SendOneMessageToProcmon(message);
                return;
            }

            var activityId = $"[{Guid.NewGuid():D}]: ";
            int offset = 0;
            while (offset < message.Length) {
                int length = Math.Min(MaxSingleMessageLength - activityId.Length, message.Length - offset);
                var buffer = activityId + message.Substring(offset, length);
                offset += length;
                SendOneMessageToProcmon(buffer);
            }
        }

        private void SendOneMessageToProcmon(string message)
        {
            Debug.Assert(message.Length <= MaxSingleMessageLength);
            uint bytesReturned = 0;
            if (!NativeMethods.DeviceIoControl(hProcmonDevice, IoCtlCode, message, (uint)(message.Length * 2), null, 0u,
                ref bytesReturned, IntPtr.Zero)) {
                int err = Marshal.GetLastWin32Error();
                if (err == 0x57) {
                    throw new InvalidOperationException("Either Procmon is not running or tracing is disabled (error code: 0x57).");
                } 
                throw new InvalidOperationException($"Failed to write to procmon device: 0x{err:X}");
            }
        }

        public void Dispose()
        {
            hProcmonDevice.Dispose();
        }
    }
}
