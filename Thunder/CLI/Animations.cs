using System;
using System.Collections.Generic;
using System.Text;

namespace Thunder.Client.CLI
{
    static internal class Animations
    {
        internal static void PrintStartMessage()
        {
            Console.CursorLeft = 0;
            Console.WriteLine("Messenger Thunder ver 1.1");
            Console.WriteLine("(c) Mironiji, 2026");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Type 'help' for commands.");
            Console.ForegroundColor = ConsoleColor.White;
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
                0, 1, 2, 3, 5, 5, 4, 3, 2, 0, 0, 1, 2, 3, 5
            };

            int startX = Console.CursorLeft;
            int startY = Console.CursorTop;

            bool cursorVisible = Console.CursorVisible;
            Console.CursorVisible = false;

            try
            {
                for (int frame = 0; frame < visibleSegmentsByFrame.Length; frame++)
                {
                    // Туча
                    for (int i = 0; i < cloud.Length; i++)
                    {
                        PrintClipped(
                            startX,
                            startY + i,
                            cloud[i],
                            ConsoleColor.DarkGray
                        );
                    }

                    // Молния
                    int lightningY = startY + cloud.Length;

                    for (int i = 0; i < lightningFull.Length; i++)
                    {
                        string line = i < visibleSegmentsByFrame[frame]
                            ? lightningFull[i]
                            : new string(' ', lightningFull[i].Length);

                        PrintClipped(
                            startX,
                            lightningY + i,
                            line,
                            ConsoleColor.Yellow
                        );
                    }

                    Thread.Sleep(40);
                }
            }
            finally
            {
                Console.ResetColor();
                Console.CursorVisible = cursorVisible;
            }
        }

        private static void PrintClipped(
            int x,
            int y,
            string text,
            ConsoleColor color)
        {
            int left = Console.WindowLeft;
            int top = Console.WindowTop;

            int right = Math.Min(
                left + Console.WindowWidth,
                Console.BufferWidth
            ) - 1;

            int bottom = Math.Min(
                top + Console.WindowHeight,
                Console.BufferHeight
            );

            // Строка за пределами окна
            if (y < top || y >= bottom || x >= right)
                return;

            // Обрезаем часть строки слева
            if (x < left)
            {
                int skip = left - x;

                if (skip >= text.Length)
                    return;

                text = text[skip..];
                x = left;
            }

            // Обрезаем строку справа
            int availableWidth = right - x;
            int length = Math.Min(text.Length, availableWidth);

            if (length <= 0)
                return;

            try
            {
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = color;
                Console.Write(text[..length]);
            }
            catch (ArgumentOutOfRangeException)
            {
                // Размер консоли мог измениться во время отрисовки
            }
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
