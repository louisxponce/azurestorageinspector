﻿using Integrations.Storage.Inspector.Services;
using Microsoft.Extensions.Hosting;
using Integrations.Storage.Inspector.Helpers;
using Integrations.Storage.Inspector.Models;
using System.Text.Json;

namespace Integrations.Storage.Inspector
{
    public partial class App : BackgroundService
    {
        private readonly ServiceBusService _serviceBusService;
        private readonly StorageService _storageService;
        private readonly LocalBlobService _localBlobService;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly Connections? _connections;
        private List<LTopic> _topics;
        private bool _topicsRetrieved;
        private readonly List<string> _menuPath = [];

        public App(ServiceBusService serviceBusService, StorageService storageService, LocalBlobService localBlobService, IHostApplicationLifetime hostApplicationLifetime)
        {
            _serviceBusService = serviceBusService;
            _storageService = storageService;
            _localBlobService = localBlobService;
            _hostApplicationLifetime = hostApplicationLifetime;
            _connections = LoadConnectionFromFile();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.Clear();
            if (_connections == null)
            {
                CloseDown();
                return;
            }
            AddAndPrintMenuPath("");
            if (!ConnectionSelectionMenu())
            {
                CloseDown();
                return;
            }
            
            do
            {
                Console.WriteLine();
                ColorConsole.WriteMenu("[s] Service Bus Topics");
                ColorConsole.WriteMenu("[b] Blob Storage");
                ColorConsole.WriteMenu("[f] Explore Blob Storage");
                var input = ColorConsole.Prompt().ToLower();
                switch (input)
                {
                    case "s":
                        await ServiceBusMenu();
                        break;
                    case "b":
                        await BlobStorageMenu();
                        break;
                    case "f":
                        await BlobStorageHierarchicalMenu();
                        break;
                    case "":
                        ColorConsole.WriteLineRed("Please enter a valid option.");
                        break;
                    default:
                        CloseDown();
                        return;
                    }
            }
            while (true);
        }

        private void AddAndPrintMenuPath(string s)
        {
            _menuPath.Add(s);
            PrintMenuPath();
        }
        private void TrimAndPrintMenuPath()
        {
            if (_menuPath.Count > 0)
            {
                _menuPath.RemoveAt(_menuPath.Count-1);
                PrintMenuPath();
            }
        }

        private void PrintMenuPath()
        {
            Console.WriteLine();
            foreach (var item in _menuPath)
            {
                ColorConsole.WriteGreen($"{item}/");
            }
        }

        private void CloseDown()
        {
            ColorConsole.WriteLineYellow("Closing down application...");
            _hostApplicationLifetime.StopApplication();
        }
    }
}