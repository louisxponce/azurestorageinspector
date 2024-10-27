using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Integrations.Storage.Inspector.Helpers;
using Integrations.Storage.Inspector.Models;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace Integrations.Storage.Inspector.Services
{
    public class ServiceBusService
    {
        private static AppSettings _appSettings;
        private ServiceBusClient _serviceBusClient;
        private ServiceBusAdministrationClient _serviceBusAdministrationClient;
        public ServiceBusService(IOptions<AppSettings> options, IOptions<Connections> connOptions)
        {
            _appSettings = options.Value;
        }

        public void InitializeConnection(Connection connection)
        {
            _serviceBusClient = new(connection.options.ServiceBusConnectionString);
            _serviceBusAdministrationClient = new(connection.options.ServiceBusConnectionString);
        }

        public async Task<List<LTopic>> GetTopicsWithSubscriptions()
        {
            List<LTopic> ltopics = new();
            var pages = _serviceBusAdministrationClient.GetTopicsAsync().AsPages();
            await foreach (var page in pages)
            {
                foreach (var topic in page.Values)
                {
                    LTopic ltopic = new()
                    {
                        Name = topic.Name
                    };

                    ltopic.Subscriptions = new();
                    var subscriptionPages = _serviceBusAdministrationClient.GetSubscriptionsAsync(topic.Name).AsPages();
                    await foreach(var subscriptionPage in subscriptionPages)
                    {
                        foreach (var subscription in subscriptionPage.Values)
                        {
                            LSubscription lSubscription = new()
                            {
                                Name = subscription.SubscriptionName,
                                TopicName = topic.Name,
                            };
                            ltopic.Subscriptions.Add(lSubscription);
                        }
                    }
                    ltopics.Add(ltopic);
                }
            }
            return ltopics;
        }

        public async Task<List<LServiceBusMessage>> GetDlqEntries(string topicName, string subscriptionName)
        {
            List<LServiceBusMessage> messages = new();
            var receiver = _serviceBusClient.CreateReceiver(topicName, $"{subscriptionName}/$deadletterqueue");

            long sequenceNumber = 0;
            do
            {
                var peekedMessage = await receiver.PeekMessagesAsync(_appSettings.MaxMessages, sequenceNumber);
                if (peekedMessage == null)
                {
                    break;
                } else if (peekedMessage[peekedMessage.Count-1].SequenceNumber == sequenceNumber)
                {
                    break;
                }
                foreach (var message in peekedMessage)
                {
                    var json = JsonSerializer.Serialize(message);
                    json = json.Replace("\"Body\":{}", "\"Body\":" + message.Body.ToString()); // Hack to get the body in the message
                    messages.Add(JsonSerializer.Deserialize<LServiceBusMessage>(json));
                    sequenceNumber = message.SequenceNumber;
                }
            } while (true);
            return messages;
        }
    }
}
