namespace Integrations.Storage.Inspector.Models
{
    public class LTopic
    {
        public string Name { get; set; }
        public List<LSubscription> Subscriptions { get; set; }
    }
}
