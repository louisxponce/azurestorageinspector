using System.Text.Json;

namespace Integrations.Storage.Inspector.Helpers
{
    public static class JsonPrettifyHelper
    {
        static JsonSerializerOptions options = new() { WriteIndented = true };

        public static string Prettify(string json)
        {
            using (JsonDocument jsonDocument = JsonDocument.Parse(json))
            {
                string prettyString = JsonSerializer.Serialize(jsonDocument.RootElement, options);
                return prettyString;
            }
        }

        public static void WriteLineInColor(string json)
        {
            foreach (char c in json)
            {
                switch (c)
                {
                    case '{':
                    case '}':
                    case '[':
                    case ']':
                    case ',':
                    case ':':
                    case '"':
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    default:
                        //if (char.IsDigit(c) || c == '-' || c == '.')
                        //{
                        //    Console.ForegroundColor = ConsoleColor.Cyan;
                        //}
                        //else
                        //{
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                        //}
                        break;
                }
                Console.Write(c);
            }

            // Reset the console color
            Console.ResetColor();
        }
    }
}
