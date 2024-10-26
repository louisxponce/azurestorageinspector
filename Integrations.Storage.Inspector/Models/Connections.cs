namespace Integrations.Storage.Inspector.Models
{
    public class Connections
    {
        public List<Connection> connections { get; set; }
    }

    public class Connection
    {
        public string name { get; set; }
        public Options options { get; set; }
    }

    public class Options
    {
        public string ServiceBusConnectionString { get; set; }
        public string BlobStorageConnectionString { get; set; }
    }

}

