using Integrations.Storage.Inspector.Helpers;
using Integrations.Storage.Inspector.Models;
using System.Text.Json;

namespace Integrations.Storage.Inspector
{
    public partial class App
    {
        private async Task ServiceBusMenu()
        {
            AddAndPrintMenuPath("Service Bus");
            await RetrieveTopics();
            ColorConsole.WriteLineWhite($"Listing {_topics.Count} found topic(s):");
            PrintAllTopicsAndSubs(printSubs: false);
            Console.WriteLine();
            do
            {
                ColorConsole.WriteMenu("[ ] Select a topic by index number");
                ColorConsole.WriteMenu("[l] List all topics");
                ColorConsole.WriteMenu("[e] Expand and list all topics with subscriptions");
                ColorConsole.WriteMenu("[r] Refresh data from the server");
                ColorConsole.WriteMenu("[c] Clear screen");
                ColorConsole.WriteMenu("[x] Back");
                var input = ColorConsole.Prompt().ToLower();
                if (string.IsNullOrEmpty(input) || input == "e")
                {
                    PrintAllTopicsAndSubs(printSeparator: true);
                }
                else switch (input)
                {
                    case "l":
                        PrintAllTopicsAndSubs(printSubs: false);
                        break;
                    case "r":
                        await RetrieveTopics(doRefresh: true);
                        break;
                    case "c":
                        Console.Clear();
                        break;
                    case "x":
                        TrimAndPrintMenuPath();
                        return;
                    default:
                    {
                        if (int.TryParse(input, out var idx) && idx < _topics.Count)
                        {
                            await TopicMenu(idx);
                        }
                        else
                        {
                            ColorConsole.WriteLineRed("Please enter a valid number.");
                        }

                        break;
                    }
                }
            }
            while (true);
        }

        private async Task TopicMenu(int idx)
        {
            var topic = _topics[idx];
            AddAndPrintMenuPath(topic.Name);
            Console.WriteLine();
            ColorConsole.WriteWhite($"Topic: ");
            ColorConsole.WriteGreen(topic.Name);
            ColorConsole.WriteLineWhite(", Subscriptions:");
            for (var i = 0; i < topic.Subscriptions.Count; i++)
            {
                ColorConsole.WriteLineYellow($"{i}. {topic.Subscriptions[i].Name}");
            }

            do
            {
                Console.WriteLine();
                ColorConsole.WriteMenu("[ ] Select a subscription by index number to get the DLQ (deadletter queue)");
                ColorConsole.WriteMenu("[x] Back");
                var input = ColorConsole.Prompt().ToLower();

                if (string.IsNullOrEmpty(input))
                {
                    ColorConsole.WriteLineRed("Please enter a valid option.");
                }
                else if (input == "x")
                {
                    TrimAndPrintMenuPath();
                    return;
                }
                else
                {
                    if (int.TryParse(input, out var subidx) && subidx < topic.Subscriptions.Count)
                    {
                        await DlqMenu(topic.Subscriptions[subidx]);
                    }
                    else
                    {
                        ColorConsole.WriteLineRed("Please enter a valid number.");
                    }
                }
            } while (true);
        }

        private async Task DlqMenu(LSubscription subscription)
        {
            AddAndPrintMenuPath(subscription.Name);
            // do while with menu for expand, list one, saving all or one blob, exit etc
            ColorConsole.WriteLineWhite($"Retrieving Deadletter messages for Topic: {subscription.TopicName}, Subscription: {subscription.Name}...");
            List<IntegrationEvent> integrationEventList = new();
            var list = await _serviceBusService.GetDlqEntries(subscription.TopicName, subscription.Name);
            ColorConsole.WriteLineWhite($"Retrieved {list.Count} message(s).");
            PrintDqlSummary(list);
            do
            {
                // The DQL Message contains when it was created and enqueued.
                // It should be possible to list or get Blobs that match that date in another container.
                // If it is possible, check and filter for blobs that match crated date around that time!
                Console.WriteLine();
                ColorConsole.WriteMenu("[ ] Select a Dlq Message by index number for details");
                ColorConsole.WriteMenu("[a] Show details for ALL Dlq Messages");
                ColorConsole.WriteMenu("[l] List summary of Dlq Messages");
                ColorConsole.WriteMenu("[d#] Download connected blob by index (Example: d1)");
                ColorConsole.WriteMenu("[d] Download ALL blobs");
                ColorConsole.WriteMenu("[c] Clear screen");
                ColorConsole.WriteMenu("[x] Back");
                var input = ColorConsole.Prompt().ToLower();
                if (string.IsNullOrEmpty(input))
                {
                    ColorConsole.WriteLineRed("Please enter a valid option.");
                }
                else switch (input)
                {
                    case "a":
                    {
                        for (var i = 0; i < list.Count; i++)
                        {
                            var dlqMessage = list[i];
                            ColorConsole.WriteLineYellow($"{i:00}. Enqueued Time: {dlqMessage.EnqueuedTime}. Error Description: {dlqMessage.DeadLetterErrorDescription}");
                            PrintDlqDetail(dlqMessage);
                        }

                        break;
                    }
                    case "l":
                        PrintDqlSummary(list);
                        break;
                    default:
                    {
                        if (input.Length > 1 && input.StartsWith("d", StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (int.TryParse(input.Substring(1), out var idx) && idx < list.Count)
                            {
                                await DownloadConnectedBlob(list[idx]);
                            }
                            else
                            {
                                ColorConsole.WriteLineRed("Please enter a valid option.");
                            }
                        }
                        else switch (input)
                        {
                            case "d":
                                ColorConsole.WriteLineYellow("Not implemented :)");
                                break;
                            case "c":
                                Console.Clear();
                                break;
                            case "x":
                                TrimAndPrintMenuPath();
                                return;
                            default:
                            {
                                if (int.TryParse(input, out var idx) && idx < list.Count)
                                {
                                    PrintDlqDetail(list[idx]);
                                }
                                else
                                {
                                    ColorConsole.WriteLineRed("Please enter a valid option.");
                                }

                                break;
                            }
                        }

                        break;
                    }
                }

            } while (true);
        }

        private async Task RetrieveTopics(bool doRefresh = true)
        {
            if (_topicsRetrieved && !doRefresh)
            {
                ColorConsole.WriteYellow("Topics are in cache. Refresh? [y]es, [Enter] to use cache: ");
                var input = ColorConsole.Prompt().ToLower();
                doRefresh = input == "y";
            }
            if (doRefresh)
            {
                Console.WriteLine();
                ColorConsole.WriteWhite("Retrieving all topics and their subscriptions... ");
                _topics = await _serviceBusService.GetTopicsWithSubscriptions();
                ColorConsole.WriteLineWhite("Done.");
                _topicsRetrieved = true;
            }
        }

        private void PrintAllTopicsAndSubs(bool printSubs = true, bool printSeparator = false)
        {
            for (var i = 0; i < _topics.Count; i++)
            {
                PrintTopicAndSubs(_topics[i], $"{i:00}. ", printSubs);
                if (printSeparator)
                {
                    ColorConsole.WriteLineWhite("---------------------------------------------------------------------");
                }
            }
        }

        private static void PrintTopicAndSubs(LTopic topic, string indexLabel = "", bool printSubs = false)
        {
            ColorConsole.WriteLineYellow($"{indexLabel}{topic.Name}");
            if (!printSubs) return;
            for (var j = 0; j < topic.Subscriptions.Count; j++)
            {
                ColorConsole.WriteLineWhite($"    {j:00)}. {topic.Subscriptions[j].Name}");
            }
        }

        private static void PrintDqlSummary(List<LServiceBusMessage> list)
        {
            for (var i = 0; i < list.Count; i++)
            {
                var dlqMessage = list[i];
                ColorConsole.WriteLineYellow($"{i:00}. Enqueued Time: {dlqMessage.EnqueuedTime}. Error Description: {dlqMessage.DeadLetterErrorDescription}");
            }
        }

        private static void PrintDlqDetail(LServiceBusMessage message)
        {
            var json = JsonSerializer.Serialize(message);
            json = JsonPrettifyHelper.Prettify(json);
            JsonPrettifyHelper.WriteLineInColor(json);
            Console.WriteLine();
        }

        private async Task DownloadConnectedBlob(LServiceBusMessage message)
        {
            HashSet<string> downloadedBlobs = [];
            //foreach (var item in list)
            //{
            //if (!downloadedBlobs.Contains(integrationEvent.EventBlobInformation.BlobPath))
            //{
            var integrationEvent = message.Body;
            ColorConsole.WriteLineWhite("Downloading blob:");
            ColorConsole.WriteLineYellow(integrationEvent.EventBlobInformation.BlobPath);
            Console.WriteLine();
            var blobContent = await _storageService.GetBlobContent(integrationEvent.EventBlobInformation.Container, integrationEvent.EventBlobInformation.BlobPath);
            var localPath = _localBlobService.SaveBlob(integrationEvent.EventBlobInformation.BlobPath, blobContent);
            downloadedBlobs.Add(integrationEvent.EventBlobInformation.BlobPath);
            ColorConsole.WriteLineWhite($"Blob successfully saved to {localPath}");
            //}
            //}
        }
    }
}
