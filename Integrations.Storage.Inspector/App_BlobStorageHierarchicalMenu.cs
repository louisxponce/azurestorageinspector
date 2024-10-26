using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Azure;
using Integrations.Storage.Inspector.Helpers;
using Microsoft.Extensions.Hosting;
using System.Text;
using System.Buffers;

namespace Integrations.Storage.Inspector
{
    public partial class App : BackgroundService
    {
        private async Task BlobStorageHierarchicalMenu()
        {
            AddAndPrintMenuPath("Explore Blob Storage");
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
                    //await BlobDownloadMenu
                    await DirectoryMenu(list[idx]);
                }
                if (input == "X")
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

        private async Task DirectoryMenu(string containerName)
        {
            List<string> directoryPath = new();
            string prefix = "";
            ColorConsole.WriteMenu("Options");
            ColorConsole.WriteMenu("Enter a directory path. Leave empty for root");
            ColorConsole.WriteMenu("[x] Back");
            _storageService.SetContainerClient(containerName);
            var list = await _storageService.GetDirectoryListing(containerName, prefix);
            foreach (var item in list)
            {
                ColorConsole.WriteLineYellow(item);
            }

            do
            {
                var inputArgs = ColorConsole.Prompt(GetDirectoryPathString(directoryPath)).Split(" ");
                var input = "";
                if (inputArgs[0].ToLower() == "cd")
                {
                    if (inputArgs.Length >= 1)
                    {
                        input = inputArgs[1];
                    }
                }
                if (input.Trim() == "..")
                {
                    if (directoryPath.Count > 0)
                    {
                        directoryPath.RemoveAt(directoryPath.Count - 1);
                    }
                }
                else if (!string.IsNullOrEmpty(input))
                {
                    //input += input;
                    directoryPath.Add(input);
                }
                
                list = await _storageService.GetDirectoryListing(containerName, GetDirectoryPathString(directoryPath));
                foreach (var item in list)
                {
                    var arr = item.Split("/");
                    var last = arr[arr.Length - 2];
                    ColorConsole.WriteLineYellow(last);
                }
            } while (true);

        }

        private static string GetDirectoryPathString(List<string> list)
        {
            StringBuilder sb = new();
            foreach (var item in list)
            {
                sb.Append(item);
                sb.Append("/");
            }
            return sb.ToString();
        }
    }
}
