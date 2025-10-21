using System.Runtime.InteropServices;
using op_homework_snake_game.UI;

namespace op_homework_snake_game 
{
    class Program
    {
        [DllImport("libc", SetLastError = true)]
        private static extern int tcgetattr(int fd, out Termios termios);

        [DllImport("libc", SetLastError = true)]
        private static extern int tcsetattr(int fd, int optionalActions, ref Termios termios);

        private const int StdinFileno = 0;
        private const int Tcsanow = 0;

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

        private static Termios _oldt;

        static void EnableRawMode()
        {
            tcgetattr(StdinFileno, out _oldt);
            var newt = _oldt;
            newt.c_lflag &= ~(2u | 8u); // ICANON (2) + ECHO (8)
            tcsetattr(StdinFileno, Tcsanow, ref newt);
        }

        static void DisableRawMode()
        {
            tcsetattr(StdinFileno, Tcsanow, ref _oldt);
        }

        static void Main()
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
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"BADERROR: {ex.Message}");
                Console.WriteLine($"{ex.StackTrace}");
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