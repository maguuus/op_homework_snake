using System;
using System.Runtime.InteropServices;
using System.Threading;
using SnakeGame;

namespace SnakeGame 
{
    class Program
    {
        [DllImport("libc", SetLastError = true)]
        private static extern int tcgetattr(int fd, out Termios termios);

        [DllImport("libc", SetLastError = true)]
        private static extern int tcsetattr(int fd, int optional_actions, ref Termios termios);

        private const int STDIN_FILENO = 0;
        private const int TCSANOW = 0;

        [StructLayout(LayoutKind.Sequential)]
        private struct Termios
        {
            public uint c_iflag;
            public uint c_oflag;
            public uint c_cflag;
            public uint c_lflag;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] c_cc;

            public uint c_ispeed;
            public uint c_ospeed;
        }

        private static Termios oldt;

        static void EnableRawMode()
        {
            tcgetattr(STDIN_FILENO, out oldt);
            var newt = oldt;
            newt.c_lflag &= ~(2u | 8u); // ICANON (2) + ECHO (8)
            tcsetattr(STDIN_FILENO, TCSANOW, ref newt);
        }

        static void DisableRawMode()
        {
            tcsetattr(STDIN_FILENO, TCSANOW, ref oldt);
        }
        
        public static void DrawPixel(int x, int y, ConsoleColor color, char symbol = '█')
        {
            if (x >= 0 && y >= 0 && x < Console.WindowWidth && y < Console.WindowHeight)
            {
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = color;
                Console.Write(symbol);
            }
        }
        
        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            Console.Clear();
            EnableRawMode();
            Console.Title = "Snake Game";
            try
            {
                Menu menu = new Menu();
                menu.Start();
            }
            catch (Exception ex)
            {
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"BADERROR: {ex.Message}");
                Console.ResetColor();
                Console.ReadKey();
            }
            finally
            {
                DisableRawMode();
                Console.CursorVisible = true;
            }
        }
    }
}