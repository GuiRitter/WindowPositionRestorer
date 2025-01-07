using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using HWND = System.IntPtr;

// https://stackoverflow.com/a/43640787/1781376
// https://stackoverflow.com/a/9669149/1781376
// https://stackoverflow.com/a/42306412/1781376
// https://stackoverflow.com/a/13745833/1781376

namespace WindowPositionRestorer
{
    internal class Program
    {
        /// <summary>Returns a dictionary that contains the handle and title of all the open windows.</summary>
        /// <returns>A dictionary that contains the handle and title of all the open windows.</returns>
        public static IDictionary<HWND, string> GetOpenWindows()
        {
            HWND shellWindow = GetShellWindow();
            Dictionary<HWND, string> windows = new Dictionary<HWND, string>();

            EnumWindows(delegate (HWND hWnd, int lParam)
            {
                if (hWnd == shellWindow) return true;
                if (!IsWindowVisible(hWnd)) return true;

                int length = GetWindowTextLength(hWnd);
                if (length == 0) return true;

                StringBuilder builder = new StringBuilder(length);
                GetWindowText(hWnd, builder, length + 1);

                windows[hWnd] = builder.ToString();
                return true;

            }, 0);

            return windows;
        }

        private delegate bool EnumWindowsProc(HWND hWnd, int lParam);

        [DllImport("USER32.DLL")]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowText(HWND hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowTextLength(HWND hWnd);

        [DllImport("USER32.DLL")]
        private static extern bool IsWindowVisible(HWND hWnd);

        [DllImport("USER32.DLL")]
        private static extern IntPtr GetShellWindow();

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        public static extern bool LockWorkStation();

        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_NOZORDER = 0x0004;


        private static string OutputPath = Path.Combine(new string[] { "C:\\", "window_position_restorer.json" });

        public static IEnumerable<WindowsWindow> GetWindowList()
        {
            return GetOpenWindows().Select
            (
                (window) =>
                {
                    IntPtr handle = window.Key;
                    string title = window.Value;
                    Rect rect = new Rect();

                    GetWindowRect(handle, ref rect);

                    return new WindowsWindow(handle, rect);
                }
            );
        }

        static void StoreAndLock()
        {
            var windowList = GetWindowList();

            var lines = (new string[] { JsonConvert.SerializeObject(windowList) }).ToList();

            File.WriteAllLines(OutputPath, lines);

            LockWorkStation();
        }
        static void Restore()
        {
            var lines = File.ReadAllLines(OutputPath);

            var json = string.Join("", lines);

            var windowStoredList = JsonConvert.DeserializeObject<IEnumerable<WindowsWindow>>(json);

            var windowCurrentList = GetWindowList();

            foreach (var windowCurrent in windowCurrentList)
            {
                var windowStoredFilteredList = windowStoredList.Where(wWindowStored => wWindowStored.Handle == windowCurrent.Handle);

                if (windowStoredFilteredList.Count() != 1)
                {
                    continue;
                }

                var windowStored = windowStoredFilteredList.Single();

                // Move the window to (0,0) without changing its size or position
                // in the Z order.
                SetWindowPos(windowCurrent.Handle, IntPtr.Zero, windowStored.Left, windowStored.Top, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
            }
        }

        static void Main(string[] args)
        {
            var input = Microsoft.VisualBasic.Interaction.MsgBox("\"Yes\" to Store and Lock\n\"No\" to Restore", MsgBoxStyle.YesNoCancel);

            if (input == MsgBoxResult.Cancel)
            {
                return;
            }

            switch (input)
            {
                case MsgBoxResult.Yes:
                    StoreAndLock();
                    return;

                case MsgBoxResult.No:
                    Restore();
                    return;

                default: return;
            }
        }
    }
}
