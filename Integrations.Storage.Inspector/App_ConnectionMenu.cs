﻿using Integrations.Storage.Inspector.Helpers;
using Microsoft.Extensions.Hosting;

namespace Integrations.Storage.Inspector
{
    public partial class App
    {
        private bool ConnectionSelectionMenu()
        {
            ColorConsole.WriteMenu("[ ] Select a connection by index number");
            for (var i = 0; i < _connections?.connections.Count; i++)
            {
                ColorConsole.WriteLineYellow($"{i}. {_connections.connections[i].name}");
            }
            do
            {
                var input = ColorConsole.Prompt().ToLower();
                if (input == "x")
                {
                    return false;
                }

                if (int.TryParse(input, out var i) && i < _connections?.connections.Count)
                {
                    var connection = _connections.connections[i];
                    ColorConsole.WriteLineYellow($"Setting up services for {connection.name}");
                    _serviceBusService.InitializeConnection(connection);
                    _storageService.InitializeConnection(connection);
                    AddAndPrintMenuPath(connection.name);
                    return true;
                }
                ColorConsole.WriteLineRed("Please enter a valid number.");
            } while (true);
        }
    }
}
