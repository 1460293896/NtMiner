﻿using System;
using System.Runtime.InteropServices;

namespace NTMiner {
    public static class NTMinerConsole {
        private static class SafeNativeMethods {
            private const string Kernel32DllName = "kernel32.dll";
            [DllImport(Kernel32DllName)]
            internal static extern bool AllocConsole();
            [DllImport(Kernel32DllName)]
            internal static extern bool FreeConsole();
            [DllImport(Kernel32DllName)]
            internal static extern IntPtr GetConsoleWindow();

            [DllImport(Kernel32DllName, SetLastError = true)]
            internal static extern IntPtr GetStdHandle(int hConsoleHandle);
            [DllImport(Kernel32DllName, SetLastError = true)]
            internal static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint mode);
            [DllImport(Kernel32DllName, SetLastError = true)]
            internal static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint mode);
            [DllImport("user32.dll", SetLastError = true)]
            internal static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);
        }

        private static void DisbleQuickEditMode() {
            const int STD_INPUT_HANDLE = -10;
            const uint ENABLE_PROCESSED_INPUT = 0x0001;
            const uint ENABLE_QUICK_EDIT_MODE = 0x0040;
            const uint ENABLE_INSERT_MODE = 0x0020;

            IntPtr hStdin = SafeNativeMethods.GetStdHandle(STD_INPUT_HANDLE);
            uint mode;
            SafeNativeMethods.GetConsoleMode(hStdin, out mode);
            mode &= ~ENABLE_PROCESSED_INPUT;//禁用ctrl+c
            mode &= ~ENABLE_QUICK_EDIT_MODE;//移除快速编辑模式
            mode &= ~ENABLE_INSERT_MODE;    //移除插入模式
            SafeNativeMethods.SetConsoleMode(hStdin, mode);
        }

        public static IntPtr Alloc() {
            IntPtr console = SafeNativeMethods.GetConsoleWindow();
            if (console == IntPtr.Zero) {
                SafeNativeMethods.AllocConsole();
                DisbleQuickEditMode();
                console = SafeNativeMethods.GetConsoleWindow();
                SafeNativeMethods.ShowWindow(console, 0);
            }
            return console;
        }

        public static IntPtr Show() {
            IntPtr console = SafeNativeMethods.GetConsoleWindow();
            if (console != IntPtr.Zero) {
                SafeNativeMethods.ShowWindow(console, 1);
            }
            return console;
        }

        public static void Hide() {
            IntPtr console = SafeNativeMethods.GetConsoleWindow();
            if (console != IntPtr.Zero) {
                SafeNativeMethods.ShowWindow(console, 0);
            }
        }

        public static void Free() {
            IntPtr console = SafeNativeMethods.GetConsoleWindow();
            if (console != IntPtr.Zero) {
                SafeNativeMethods.FreeConsole();
            }
        }
    }
}
