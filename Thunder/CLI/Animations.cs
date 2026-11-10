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
        0, 1, 2, 3, 5, 5, 4, 3, 2, 0
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
                    for (int i = 0; i < cloud.Length; i++)
                    {
                        DrawLine(startX, startY + i,
                            cloud[i].PadRight(width));
                    }

                    int visible = visibleSegmentsByFrame[frame];
                    bool appearing = frame <= 5;

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

                    Thread.Sleep(100);
                }

                // Продолжаем вывод после области анимации.
                Console.SetCursorPosition(0, bottomY);
                Console.WriteLine();
            }
            finally
            {
                Console.CursorVisible = cursorVisible;
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
