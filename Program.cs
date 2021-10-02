using System;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace json_terminal_explorer
{
    public class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length != 1 && !CanReadFromStdin())
            {
                Console.WriteLine("Usage: [jte path/to.json] or [json-command | jte]");
                return 1;
            }

            if (args.Length == 1)
            {
                var filename = Path.GetFullPath(args[0]);
                var json = JsonDocument.Parse(File.OpenRead(filename), JsonDocumentOptions());
                Console.WriteLine(JsonDocToString(json));
            }
            else
            {
                var json = JsonDocument.Parse(Console.OpenStandardInput(), JsonDocumentOptions());
                Console.WriteLine(JsonDocToString(json));
            }

            return 0;
        }

        private static JsonDocumentOptions JsonDocumentOptions() => new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip,
        };

        private static string JsonDocToString(JsonDocument doc)
        {
            using var stream = new MemoryStream();
            var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
            {
                Indented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            doc.WriteTo(writer);
            writer.Flush();
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        private static bool CanReadFromStdin()
        {
            try
            {
                return Console.KeyAvailable;
            }
            catch (InvalidOperationException)
            {
                // https://docs.microsoft.com/en-us/dotnet/api/system.console.keyavailable?view=net-5.0#exceptions
                // "Standard input is redirected to a file instead of the keyboard" which is what we want
                return true;
            }
        }
    }
}
