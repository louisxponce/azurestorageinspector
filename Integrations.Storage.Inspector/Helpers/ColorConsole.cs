namespace Integrations.Storage.Inspector.Helpers
{
    public static class ColorConsole
    {
        public static void WriteLineWhite(string s)
        {
            PrivateWrite(WriteType.ConsoleWriteLine, ConsoleColor.White, s);
        }

        public static void WriteLineGreen(string s)
        {
            PrivateWrite(WriteType.ConsoleWriteLine, ConsoleColor.Green, s);
        }

        public static void WriteLineYellow(string s)
        {
            PrivateWrite(WriteType.ConsoleWriteLine, ConsoleColor.Yellow, s);
        }
        public static void WriteLineRed(string s)
        {
            PrivateWrite(WriteType.ConsoleWriteLine, ConsoleColor.Red, s);
        }
        public static void WriteWhite(string s)
        {
            PrivateWrite(WriteType.ConsoleWrite, ConsoleColor.White, s);
        }

        public static void WriteGreen(string s)
        {
            PrivateWrite(WriteType.ConsoleWrite, ConsoleColor.Green, s);
        }

        public static void WriteYellow(string s)
        {
            PrivateWrite(WriteType.ConsoleWrite, ConsoleColor.Yellow, s);
        }
        public static void WriteRed(string s)
        {
            PrivateWrite(WriteType.ConsoleWrite, ConsoleColor.Red, s);
        }
        public static void WriteMenu(string s)
        {
            //Console.BackgroundColor = ConsoleColor.Green;
            //Console.ForegroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(s);
            Console.ResetColor();
        }

        public static string Prompt(string s = "")
        {
            PrivateWrite(WriteType.ConsoleWrite, ConsoleColor.Green, $"{s}> ");
            return Console.ReadLine() ?? "";
        }

        private static void PrivateWrite(WriteType type, ConsoleColor color, string s)
        {
            Console.ForegroundColor = color;
            switch (type)
            {
                case WriteType.ConsoleWriteLine:
                    Console.WriteLine(s);
                    break;
                case WriteType.ConsoleWrite:
                    Console.Write(s);
                    break;
                default:
                    break;
            }
            Console.ResetColor();
        }
    }

    enum WriteType
    {
        ConsoleWriteLine,
        ConsoleWrite
    }
}
