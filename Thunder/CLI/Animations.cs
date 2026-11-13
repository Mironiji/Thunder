using System;
using System.Collections.Generic;
using System.Text;

// todo очищать полностью экран и переходить в (0, 0) при выборе вкладок
namespace Thunder.Client.CLI
{
    static internal class Animations
    {
        private static void PrintClored(string m, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(m);
            Console.ForegroundColor = ConsoleColor.White;
        }
        internal static void PrintRed(string m) => PrintClored(m, ConsoleColor.Red);
        internal static void PrintGreen(string m) => PrintClored(m, ConsoleColor.Green);
        internal static void PrintDarkBlue(string m) => PrintClored(m, ConsoleColor.DarkBlue);
        internal static void PrintMagenta(string m) => PrintClored(m, ConsoleColor.Magenta);
        internal static void PrintYellow(string m) => PrintClored(m, ConsoleColor.Yellow);
        internal static void PrintLightMessage(string m) => PrintClored('☇' + m + '☇', ConsoleColor.Yellow);
        internal static void PrintStartMessage()
        {
            Console.CursorLeft = 0;
            Console.WriteLine("Messenger Thunder ver 1.1");
            Console.WriteLine("(c) Mironiji, 2026");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Type 'help' for commands.");
            Console.ForegroundColor = ConsoleColor.White;
        }

        internal static void PrintUnAuthtorizedMenu()
        {
            Console.CursorLeft = 0;
            Console.WriteLine("Messenger Thunder ver 1.1");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\t-- login\n\t-- signup\n\t-- exit");
        }

        internal static void PrintAuthtorizedMenu()
        {
            Console.CursorLeft = 0;
            Console.WriteLine("Messenger Thunder ver 1.1");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\t-- type *username* to start typing a message" +
                "\n\t-- find *userna...* to get all usernames starts with *userna...*\n\t" +
                "-- exit");
        }

        internal static void PrintThunderAnimation()
        {
            string[] cloud =
            {
        "      .--.      ",
        "   .-(    ).    ",
        "  (___.__)__)   "
    };

            string[] lightningFull =
            {
        "       /        ",
        "      /         ",
        "       \\        ",
        "      /         ",
        "     /          "
    };

            int[] visibleSegmentsByFrame =
            {
                0, 1, 2, 3, 5, 5, // Молния появляется
                4, 3, 2, 1, 0,    // Исчезает сверху вниз
                1, 2, 3, 5        // Появляется снова и остаётся
            };

            int height = cloud.Length + lightningFull.Length;
            int width = Math.Max(
                cloud.Max(s => s.Length),
                lightningFull.Max(s => s.Length)
            );

            if (Console.WindowHeight < height)
            {
                Console.WriteLine("Увеличьте высоту окна консоли.");
                return;
            }

            bool cursorVisible = Console.CursorVisible;

            try
            {
                Console.CursorVisible = false;

                // Начинаем с новой строки, если курсор
                // находится после другого текста.
                if (Console.CursorLeft != 0)
                    Console.WriteLine();

                int startX = 0;

                // Резервируем место под все строки анимации.
                // Если места в окне нет, консоль прокрутится.
                for (int i = 1; i < height; i++)
                    Console.WriteLine();

                int bottomY = Console.CursorTop;
                int startY = bottomY - height + 1;

                for (int frame = 0;
                     frame < visibleSegmentsByFrame.Length;
                     frame++)
                {
                    // Облако присутствует на каждом кадре.
                    Console.ForegroundColor = ConsoleColor.Gray;
                    for (int i = 0; i < cloud.Length; i++)
                    {
                        DrawLine(startX, startY + i,
                            cloud[i].PadRight(width));
                    }

                    int visible = visibleSegmentsByFrame[frame];
                    bool appearing = frame <= 5 || frame <= 11;

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    for (int i = 0; i < lightningFull.Length; i++)
                    {
                        bool show = appearing
                            ? i < visible
                            : i >= lightningFull.Length - visible;

                        string line = show
                            ? lightningFull[i]
                            : "";
                        DrawLine(
                            startX,
                            startY + cloud.Length + i,
                            line.PadRight(width)
                        );
                    }

                    Thread.Sleep(40);
                }

                // Продолжаем вывод после области анимации.
                Console.SetCursorPosition(0, bottomY);
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.White;
            }
            finally
            {
                Console.CursorVisible = cursorVisible;
            }
        }

        internal static void PintHorizontalLightAnimation()
        {
            if (Console.IsOutputRedirected)
                return;

            string[] lightning =
            {
                @"        /__    /____",
                @"   __/\_/\___/\_____",
                @"__/      \    \_____",
                @"          \__"
            };

            const int travel = 4;
            const int columnsPerFrame = 2;
            const int frameDelay = 16;
            int width = lightning.Max(line => line.Length);
            int frameWidth = width + travel;
            int totalSteps = width * 2 + travel;
            ConsoleColor previousColor = Console.ForegroundColor;
            bool cursorWasVisible = OperatingSystem.IsWindows() && Console.CursorVisible;

            try
            {
                if (OperatingSystem.IsWindows())
                    Console.CursorVisible = false;

                if (Console.CursorLeft != 0)
                    Console.WriteLine();

                // Даём консоли прокрутиться до того, как запомним координаты.
                for (int row = 1; row < lightning.Length; row++)
                    Console.WriteLine();

                int bottomY = Console.CursorTop;
                int startY = bottomY - lightning.Length + 1;
                Console.ForegroundColor = ConsoleColor.Yellow;

                int progress = 0;
                while (progress < totalSteps)
                {
                    progress = Math.Min(progress + columnsPerFrame, totalSteps);

                    // Сначала открываем рисунок, затем сдвигаем и стираем его слева.
                    int visibleEnd = Math.Min(progress, width);
                    int offset = Math.Clamp(progress - width, 0, travel);
                    int visibleStart = Math.Max(0, progress - width - travel);

                    int left = Console.WindowLeft;
                    int top = Console.WindowTop;
                    int bottom = Math.Min(top + Console.WindowHeight, Console.BufferHeight);
                    int visibleWidth = Math.Clamp(Console.WindowWidth - 1, 0, frameWidth);

                    for (int row = 0; row < lightning.Length; row++)
                    {
                        int y = startY + row;
                        if (y < top || y >= bottom)
                            continue;

                        string visible = lightning[row].PadRight(width)[visibleStart..visibleEnd];
                        string line = new string(' ', offset + visibleStart) + visible;
                        DrawLine(left, y, line.PadRight(frameWidth)[..visibleWidth]);
                    }

                    Thread.Sleep(frameDelay);
                }

                Console.SetCursorPosition(Console.WindowLeft, Math.Min(bottomY, Console.BufferHeight - 1));
                Console.WriteLine();
            }
            finally
            {
                Console.ForegroundColor = previousColor;
                if (OperatingSystem.IsWindows())
                    Console.CursorVisible = cursorWasVisible;
            }
        }

        private static void DrawLine(int x, int y, string line)
        {
            // Не пишем в последнюю колонку буфера:
            // это предотвращает перенос строки.
            int available = Console.BufferWidth - x - 1;

            if (available <= 0)
                return;

            Console.SetCursorPosition(x, y);

            int length = Math.Min(line.Length, available);
            Console.Write(line.AsSpan(0, length));
        }

        internal static void PrintHelp()
        {
            Console.WriteLine("Enter command: ");
            foreach (var command in Enum.GetValues<Command>())
            {
                Console.WriteLine($"\t--{command}");
            }

            Console.Write("Try to send ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("thunder ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("to your friend!\n");
        }
    }
}
