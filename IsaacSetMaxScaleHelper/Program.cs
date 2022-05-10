using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace IsaacSetMaxScaleHelper
{
    internal class Program
    {
        // server address + port
        private const string LOCALHOST = "127.0.0.1";
        private const int PORT = 11568;

        // current as of Repentance (found with Spy++)
        private const string ISAAC_CLASSNAME = "GLFW30";
        private const string ISAAC_WINDOWNAME = "Binding of Isaac: Repentance";

        // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-findwindowa
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowrect
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        // used by GetWindowRect
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowpos
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        // flags defined for SetWindowPos
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_NOZORDER = 0x0004;

        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Attempting to start server on {0}:{1}", LOCALHOST, PORT);

                TcpListener server = new TcpListener(IPAddress.Parse(LOCALHOST), PORT);
                server.Start();
                
                Console.WriteLine("Server started on {0}:{1}", LOCALHOST, PORT);

                while (true)
                {
                    byte[] buffer = new byte[50];

                    using (TcpClient client = await server.AcceptTcpClientAsync())
                    {
                        Console.WriteLine("Accepted connection");

                        using (NetworkStream stream = client.GetStream())
                        {
                            await stream.ReadAsync(buffer);
                        }
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, buffer.Length).TrimEnd('\0');
                    Console.WriteLine("Message: " + message);

                    if (message == "IsaacSetMaxScaleHelper")
                    {
                        ToggleWindowSize();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Server error");
                Console.WriteLine(ex.ToString());
            }
        }

        private static void ToggleWindowSize()
        {
            try
            {
                IntPtr hWnd = FindWindow(ISAAC_CLASSNAME, ISAAC_WINDOWNAME);

                if (hWnd != IntPtr.Zero)
                {
                    Console.WriteLine("Found Isaac window: {0}, {1}", ISAAC_CLASSNAME, ISAAC_WINDOWNAME);

                    if (GetWindowRect(hWnd, out RECT lpRect))
                    {
                        int width = lpRect.Right - lpRect.Left;
                        int height = lpRect.Bottom - lpRect.Top;

                        if (SetWindowPos(hWnd, IntPtr.Zero, 0, 0, width - 1, height, SWP_NOMOVE | SWP_NOACTIVATE | SWP_NOZORDER) &&
                            SetWindowPos(hWnd, IntPtr.Zero, 0, 0, width, height, SWP_NOMOVE | SWP_NOACTIVATE | SWP_NOZORDER))
                        {
                            Console.WriteLine("Toggled window size");
                        }
                        else
                        {
                            Console.WriteLine("Unable to toggle window size");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Unable to toggle window size");
                    }
                }
                else
                {
                    Console.WriteLine("Unable to find Isaac window: {0}, {1}", ISAAC_CLASSNAME, ISAAC_WINDOWNAME);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Windows API error");
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
