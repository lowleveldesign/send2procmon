using System;
using System.Collections.Generic;
using System.Reflection;

namespace LowLevelDesign.Send2Procmon
{
    static class Program
    {
        public static void Main(string[] args)
        {
            if (ShouldIPrintHelpAndExit(args)) {
                PrintHelp();
                return;
            }

            var messages = ParseMessages(args);

            ProcmonLogger procmonLogger = null;
            try {
                procmonLogger = new ProcmonLogger();

                procmonLogger.SendToProcmon(messages);
            } catch (InvalidOperationException ex) {
                Console.Error.WriteLine(ex.Message);
            } finally {
                if (procmonLogger != null) {
                    procmonLogger.Dispose();
                }
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
                if (!IsPipedInput()) {
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

        private static bool IsPipedInput()
        {
            try {
                bool isKey = Console.KeyAvailable;
                return false;
            } catch {
                return true;
            }
        }

    }
}
