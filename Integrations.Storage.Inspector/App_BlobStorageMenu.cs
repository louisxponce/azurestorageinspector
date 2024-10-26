using Integrations.Storage.Inspector.Helpers;
using Integrations.Storage.Inspector.Models;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace Integrations.Storage.Inspector
{
    public partial class App : BackgroundService
    {
        private async Task BlobStorageMenu()
        {
            AddAndPrintMenuPath("Blob Storage");
            do
            {
                Console.WriteLine();
                var list = _storageService.GetContainers();
                ColorConsole.WriteLineWhite($"Listing {list.Count} found container(s):");
                for (int i = 0; i < list.Count; i++)
                {
                    var containerName = list[i];
                    ColorConsole.WriteLineYellow($"{i}. {containerName}");
                }
                ColorConsole.WriteMenu("[ ] Select a container by index number");
                ColorConsole.WriteMenu("[x] Back");
                var input = ColorConsole.Prompt().ToUpper();
                if (int.TryParse(input, out int idx) && idx < list.Count)
                {
                    await BlobDownloadMenu(list[idx]);
                }
                else if (input == "X")
                {
                    TrimAndPrintMenuPath();
                    return;
                }
                else
                {
                    ColorConsole.WriteLineRed("Please enter a valid option.");
                }
            }
            while (true);
        }

        private async Task BlobDownloadMenu(string containerName)
        {
            //AddAndPrintMenuPath(containerName);
            _menuPath.Add(containerName);
            Console.WriteLine();
            ColorConsole.WriteWhite("Container: ");
            ColorConsole.WriteLineGreen(containerName);
            do
            {
                PrintMenuPath();
                Console.WriteLine();
                ColorConsole.WriteMenu("Options");
                ColorConsole.WriteMenu("- Download: Enter the full path to the blob");
                ColorConsole.WriteMenu("- Match/Search: Enter the beginning of the path. Terminate with a star. (Ex: BC/Products/2020-12/31/7/*)");
                ColorConsole.WriteMenu("[x] Back");
                var input = ColorConsole.Prompt();
                try
                {
                    input = input.Replace("\\\\", "\\").Replace("\\", "/");
                    if (input.EndsWith('*'))
                    {
                        var prefix = input.Replace("*", "");
                        await MultipleBlobDownloadMenu(containerName, prefix);
                    }
                    else if (input.ToUpper() == "X")
                    {
                        TrimAndPrintMenuPath();
                        return;
                    }
                    else
                    {
                        ColorConsole.WriteLineWhite("Downloading blob...");
                        Console.WriteLine();
                        await DownloadBlob(containerName, input);
                    }
                }
                catch (Exception ex)
                {
                    ColorConsole.WriteLineRed(ex.Message);
                }
                Console.WriteLine();
            }
            while (true);
        }

        private async Task MultipleBlobDownloadMenu(string containerName, string prefix)
        {
            var list = await _storageService.GetBlobList(containerName, prefix);
            ColorConsole.WriteLineWhite("Matching...");
            do
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    ColorConsole.WriteLineYellow($"{i:00}. {item}");
                }
                Console.WriteLine();
                ColorConsole.WriteMenu("[ ] Select a blob by index number for download");
                ColorConsole.WriteMenu("[d] Download ALL blobs. No print");
                ColorConsole.WriteMenu("[dw] Download ALL blobs WITH print");
                ColorConsole.WriteMenu("[x] Back");
                var input = ColorConsole.Prompt().ToUpper();
                if (string.IsNullOrEmpty(input))
                {
                    ColorConsole.WriteLineRed("Please enter a valid option.");
                }
                else if (input == "X")
                {
                    return;
                }
                else if (input == "D")
                {
                    for (int j = 0; j < list.Count; j++)
                    {
                        await DownloadBlob(containerName, list[j], false);
                        Console.WriteLine();
                    }
                }
                else if (input == "DW")
                {
                    for (int j = 0; j < list.Count; j++)
                    {
                        await DownloadBlob(containerName, list[j]);
                        Console.WriteLine();
                    }
                }
                else
                {
                    if (int.TryParse(input, out int idx) && idx < list.Count)
                    {
                        await DownloadBlob(containerName, list[idx]);
                        Console.WriteLine();
                    }
                }
            }
            while (true);
        }

        private async Task DownloadBlob(string containerName, string blobName, bool print = true)
        {
            var blobContent = await _storageService.GetBlobContent(containerName, blobName);
            var json = JsonPrettifyHelper.Prettify(blobContent);
            var localPath = _localBlobService.SaveBlob(Path.GetFileName(blobName), json);
            if (print)
            {
                JsonPrettifyHelper.WriteLineInColor(json);
            }
        }

        private static Connections? LoadConnectionFromFile()
        {
            // Load connections
            var connectionsPath = "connections.json";
            string json;
            if (!File.Exists(connectionsPath))
            {
                // Create a dummy connection andf exporting to file
                Connections conn = new()
                {
                    connections = new()
                    {
                        new()
                        {
                            name = "Connection1",
                            options = new()
                            {
                                BlobStorageConnectionString = "Endpoint=sb:/.....",
                                ServiceBusConnectionString ="DefaultEndpointsProtocol=https;AccountName=...."
                            }
                        }
                    }
                };
                json = JsonSerializer.Serialize(conn, new JsonSerializerOptions() { WriteIndented = true });
                File.WriteAllText(connectionsPath, json);
                ColorConsole.WriteLineYellow("No connections.json file was found. A template has been created. Please fill in the information and run the program again.");
                return null;
            }
            else
            {
                json = File.ReadAllText(connectionsPath);
                return JsonSerializer.Deserialize<Connections>(json);
            }
        }
    }
}
