namespace Integrations.Storage.Inspector.Models
{
    public class IntegrationEvent
    {
        public string Id { get; set; }
        public string Flow { get; set; }
        public string EntityIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public object TargetSystem { get; set; }
        public object SortingParameter { get; set; }
        public DateTime Created { get; set; }
        public int Step { get; set; }
        public Eventblobinformation EventBlobInformation { get; set; }
    }

    public class Eventblobinformation
    {
        public string BlobPath { get; set; }
        public string Container { get; set; }
        public int FileType { get; set; }
    }

}
