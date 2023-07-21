using System;
using System.Runtime.InteropServices;
using System.Text;

namespace FileMonitor
{
    public class Program
    {
        public static class NativeMethods
        {
            // Windows API constants
            public const uint FILE_NOTIFY_CHANGE_FILE_NAME = 0x00000001;
            public const uint FILE_ACTION_ADDED = 0x00000001;

            public const uint WAIT_OBJECT_0 = 0x00000000;
            public const uint WAIT_FAILED = 0xFFFFFFFF;
            public const uint INFINITE = 0xFFFFFFFF;

            // Windows API functions
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr FindFirstChangeNotification(string lpPathName, bool bWatchSubtree, uint dwNotifyFilter);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool FindNextChangeNotification(IntPtr hChangeHandle);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool FindCloseChangeNotification(IntPtr hChangeHandle);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern uint WaitForMultipleObjects(uint nCount, IntPtr[] lpHandles, bool bWaitAll, uint dwMilliseconds);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            private static extern bool ReadDirectoryChangesW(
                IntPtr hDirectory,
                IntPtr lpBuffer,
                uint nBufferLength,
                bool bWatchSubtree,
                uint dwNotifyFilter,
                out uint lpBytesReturned,
                IntPtr lpOverlapped,
                IntPtr lpCompletionRoutine
            );

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            private struct FILE_NOTIFY_INFORMATION
            {
                public uint NextEntryOffset;
                public uint Action;
                public uint FileNameLength;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
                public string FileName;
            }

            static string GetActionString(uint action)
            {
                return action switch
                {
                    1 => "Added",
                    2 => "Removed",
                    3 => "Modified",
                    4 => "Renamed (old name)",
                    5 => "Renamed (new name)",
                    _ => "Unknown",
                };
            }
            public static void MonitorFolder(string dirPath)
            {
                IntPtr hDir = FindFirstChangeNotification(dirPath, true, FILE_NOTIFY_CHANGE_FILE_NAME);

                if (hDir == IntPtr.Zero || hDir.ToInt32() == -1)
                {
                    // Handle error if FindFirstChangeNotification fails
                    Console.WriteLine("Error initializing file system monitoring.");
                    return;
                }

                // Array to hold the handle for the FindFirstChangeNotification function
                IntPtr[] handles = new IntPtr[] { hDir };

                uint waitResult;

                const int bufferSize = 4096; // Buffer size for file change information
                IntPtr buffer = Marshal.AllocHGlobal(bufferSize);

                try
                {
                    while (true)
                    {
                        waitResult = WaitForMultipleObjects(1, handles, false, INFINITE);

                        if (waitResult == WAIT_FAILED)
                        {
                            // Handle error
                            int errorCode = Marshal.GetLastWin32Error();
                            Console.WriteLine($"WaitForMultipleObjects failed with error code: {errorCode}");
                            break;
                        }

                        if (waitResult == WAIT_OBJECT_0)
                        {
                            // Get the specific change information
                            if (ReadDirectoryChangesW(hDir, buffer, bufferSize, true, FILE_NOTIFY_CHANGE_FILE_NAME, out _, IntPtr.Zero, IntPtr.Zero))
                            {
                                uint offset = 0;
                                while (true)
                                {
                                    // Get the FILE_NOTIFY_INFORMATION structure at the current offset
                                    var fni = (FILE_NOTIFY_INFORMATION)Marshal.PtrToStructure(IntPtr.Add(buffer, (int)offset), typeof(FILE_NOTIFY_INFORMATION));

                                    // Get the file name from the structure
                                    string fileName = fni.FileName;

                                    // Convert action to a meaningful string
                                    string actionString = GetActionString(fni.Action);

                                    Console.WriteLine($"File \"{fileName}\" was {actionString} in the download folder!");

                                    // Move to the next structure in the buffer
                                    if (fni.NextEntryOffset == 0)
                                        break;
                                    offset += fni.NextEntryOffset;
                                }
                            }
                            else
                            {
                                // Handle error if ReadDirectoryChangesW fails
                                int errorCode = Marshal.GetLastWin32Error();
                                Console.WriteLine($"ReadDirectoryChangesW failed with error code: {errorCode}");
                                break;
                            }

                            // Reset the event handle
                            FindNextChangeNotification(hDir);
                        }


                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                    FindCloseChangeNotification(hDir);
                }
            }
        }

        public static void Main(string[] args)
        {
#if DEBUG
            // When you want to debug
            Console.WriteLine("Monitoring the download folder...");
            NativeMethods.MonitorFolder(@"C:\Users\" + Environment.UserName + @"\Downloads\");
#endif
            // When you feel brave
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: ProgramName <dir>");
                return;
            }
            NativeMethods.MonitorFolder(args[0]);

        }
    }
}