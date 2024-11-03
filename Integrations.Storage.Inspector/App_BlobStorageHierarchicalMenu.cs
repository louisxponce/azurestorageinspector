using Integrations.Storage.Inspector.Helpers;
using System.Text;

namespace Integrations.Storage.Inspector
{
    public partial class App
    {
        private async Task BlobStorageHierarchicalMenu()
        {
            AddAndPrintMenuPath("Explore Blob Storage");
            do
            {
                Console.WriteLine();
                var list = _storageService.GetContainers();
                ColorConsole.WriteLineWhite($"Listing {list.Count} found container(s):");
                for (var i = 0; i < list.Count; i++)
                {
                    var containerName = list[i];
                    ColorConsole.WriteLineYellow($"{i}. {containerName}");
                }
                ColorConsole.WriteMenu("[ ] Select a container by index number");
                ColorConsole.WriteMenu("[x] Back");
                var input = ColorConsole.Prompt().ToLower();
                if (int.TryParse(input, out var idx) && idx < list.Count)
                {
                    await DirectoryMenu(list[idx]);
                }
                else if (input == "x")
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
            List<string> directoryPath = [];
            ColorConsole.WriteMenu("Options");
            ColorConsole.WriteMenu("Enter a directory path. Leave empty for root");
            ColorConsole.WriteMenu("[x] Back");
            _storageService.SetContainerClient(containerName);
            var list = await _storageService.GetDirectoryListing(containerName, "");
            foreach (var item in list)
            {
                ColorConsole.WriteLineYellow(item.Name);
            }

            do
            {
                var inputArgs = ColorConsole.Prompt(GetDirectoryPathString(directoryPath)).Split(" ");
                var input = "";
                if (inputArgs[0] == "x")
                {
                    return;
                }
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
                    directoryPath.Add(input);
                }
                
                list = await _storageService.GetDirectoryListing(containerName, GetDirectoryPathString(directoryPath));
                foreach (var item in list)
                {       
                    ColorConsole.WriteLineYellow(item.Name);
                }
            } while (true);
        }

        private static string GetDirectoryPathString(List<string> list)
        {
            StringBuilder sb = new();
            foreach (var item in list)
            {
                sb.Append(item);
                sb.Append('/');
            }
            return sb.ToString();
        }
    }
}
