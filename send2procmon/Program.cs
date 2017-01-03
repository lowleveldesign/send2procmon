using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LowLevelDesign.Send2Procmon
{
    static class Program
    {
        private const uint IoCtlCode = 2503311876;
        private const int MaxSingleMessageLength = 2047;

        public static void Main(string[] args)
        {
            if (ShouldIPrintHelpAndExit(args)) {
                PrintHelp();                
                return;
            }

            var messages = ParseMessages(args);
            var hProcmonDevice = NativeMethods.CreateFile(
                @"\\.\Global\ProcmonDebugLogger", NativeMethods.FileAccess.Write | NativeMethods.FileAccess.Read,
                NativeMethods.FileShare.Write, IntPtr.Zero, NativeMethods.CreationDisposition.OpenExisting,
                NativeMethods.FileAttributes.Normal, IntPtr.Zero);
            if (hProcmonDevice.IsInvalid || hProcmonDevice.IsClosed) {
                int err = Marshal.GetLastWin32Error();
                Console.Error.WriteLine("Can't connect to procmon device: 0x{0:X}. Probably procmon is not running.",
                    err);
                return;
            }
            try {
                foreach (var message in messages) {
                    SplitMessagesIfNecessaryAndSendThemToProcmon(hProcmonDevice, message);
                }
            }
            finally {
                hProcmonDevice.Dispose();
            }
        }

        private static bool ShouldIPrintHelpAndExit(string[] args)
        {
            return args.Length > 0 &&
                   (string.Equals(args[0], "-?") ||
                    string.Equals(args[0], "--help") ||
                    string.Equals(args[0], "-help") ||
                    string.Equals(args[0], "-h"));
        }

        private static void PrintHelp()
        {
            Console.WriteLine("send2procmon v{0} - sends input to procmon",
                Assembly.GetExecutingAssembly().GetName().Version.ToString());
            Console.WriteLine("Copyright (C) 2017 Sebastian Solnica (@lowleveldesign)");
            Console.WriteLine();
            Console.WriteLine("Usage: send2procmon <message-to-send>");
            Console.WriteLine();
        }

        private static string[] ParseMessages(string[] args)
        {
            string[] messages = args;
            if (args.Length == 0) {
                if (Console.In.Peek() == -1) {
                    return new string[0];
                }
                var l = new List<string>();
                string msg;
                while ((msg = Console.ReadLine()) != null) {
                    l.Add(msg);
                }
                messages = l.ToArray();
            }
            return messages;
        }

        private static void SplitMessagesIfNecessaryAndSendThemToProcmon(SafeFileHandle hProcmonDevice, string message)
        {
            if (message.Length <= MaxSingleMessageLength) {
                SendOneMessageToProcmon(hProcmonDevice, message);
                return;
            }

            var activityId = $"[{Guid.NewGuid():D}]: ";
            int offset = 0;
            while (offset < message.Length) {
                int length = Math.Min(MaxSingleMessageLength - activityId.Length, message.Length - offset);
                var buffer = activityId + message.Substring(offset, length);
                offset += length;
                SendOneMessageToProcmon(hProcmonDevice, buffer);
            }
        }

        private static void SendOneMessageToProcmon(SafeFileHandle hProcmonDevice, string message)
        {
            Debug.Assert(message.Length <= MaxSingleMessageLength);
            uint bytesReturned = 0;
            if (!NativeMethods.DeviceIoControl(hProcmonDevice, IoCtlCode, message, (uint)(message.Length*2), null, 0u,
                ref bytesReturned, IntPtr.Zero)) {
                int err = Marshal.GetLastWin32Error();
                if (err == 0x57) {
                    Console.Error.WriteLine("Either Procmon is not running or tracing is disabled (error code: 0x57).");
                } else {
                    Console.Error.WriteLine("Failed to write to procmon device: 0x{0:X}", err);
                }
            }
        }
    }
}
