// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using Vortice.Mathematics;
using Vortice.Win32;
using static Vortice.Win32.User32;

namespace VorticeImGui
{
    public class Window
    {
        private const int CW_USEDEFAULT = unchecked((int)0x80000000);

        public string Title { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public IntPtr Handle { get; private set; }

        public Window(string wndClass, string title, int width, int height)
        {
            Title = title;
            Width = width;
            Height = height;

            var screenWidth = GetSystemMetrics(SystemMetrics.SM_CXSCREEN);
            var screenHeight = GetSystemMetrics(SystemMetrics.SM_CYSCREEN);
            var x = (screenWidth - Width) / 2;
            var y = (screenHeight - Height) / 2;

            WindowStyles style = 0;
            WindowExStyles styleEx = 0;

            const bool resizable = true;
            if (resizable)
            {
                style = WindowStyles.WS_OVERLAPPEDWINDOW;
            }
            else
            {
                style = WindowStyles.WS_POPUP | WindowStyles.WS_BORDER | WindowStyles.WS_CAPTION | WindowStyles.WS_SYSMENU;
            }

            styleEx = WindowExStyles.WS_EX_APPWINDOW | WindowExStyles.WS_EX_WINDOWEDGE;
            style |= WindowStyles.WS_CLIPCHILDREN | WindowStyles.WS_CLIPSIBLINGS;

            var windowRect = new Rect(0, 0, Width, Height);
            AdjustWindowRectEx(ref windowRect, style, false, styleEx);

            var windowWidth = windowRect.Right - windowRect.Left;
            var windowHeight = windowRect.Bottom - windowRect.Top;

            var hwnd = CreateWindowEx(
                (int)styleEx, wndClass, Title, (int)style,
                x, y, windowWidth, windowHeight,
                IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            Handle = hwnd;
        }

        public void Show()
        {
            ShowWindow(Handle, ShowWindowCommand.Normal);
        }

        public void Destroy()
        {
            if (Handle != IntPtr.Zero)
            {
                DestroyWindow(Handle);
                Handle = IntPtr.Zero;
            }
        }
    }
}