using System;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Spectre.Console;

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
                PrettyPrintJson(json);
            }
            else
            {
                var json = JsonDocument.Parse(Console.OpenStandardInput(), JsonDocumentOptions());
                PrettyPrintJson(json);
            }

            return 0;
        }

        private static JsonDocumentOptions JsonDocumentOptions() => new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip,
        };

        private static void PrettyPrintJson(JsonDocument doc)
        {
            var root = new Tree("{}");
            foreach (var property in doc.RootElement.EnumerateObject())
            {
                CreateNodeForProperty(root.AddNode, property.Name, property.Value);
            }
            AnsiConsole.Render(root);
        }

        private static void CreateNodeForProperty(Func<string, TreeNode> addNode, string name, JsonElement value)
        {
            switch (value.ValueKind)
            {
                case JsonValueKind.Object:
                    var objectNode = addNode(name.EscapeMarkup());
                    foreach (var subProperty in value.EnumerateObject())
                    {
                        CreateNodeForProperty(objectNode.AddNode, subProperty.Name, subProperty.Value);
                    }
                    break;
                case JsonValueKind.Array:
                    var arrayNode = addNode(name.EscapeMarkup());
                    var i = 0;
                    foreach (var subProperty in value.EnumerateArray())
                    {
                        CreateNodeForProperty(arrayNode.AddNode, i++.ToString(), subProperty);
                    }
                    break;
                default:
                    addNode($"{name}: {value}".EscapeMarkup());
                    break;
            }
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
