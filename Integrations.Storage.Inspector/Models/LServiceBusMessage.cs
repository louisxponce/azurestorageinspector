namespace Integrations.Storage.Inspector.Models
{
    public class LServiceBusMessage
    {
        public IntegrationEvent Body { get; set; }
        public string MessageId { get; set; }
        public object PartitionKey { get; set; }
        public object TransactionPartitionKey { get; set; }
        public object SessionId { get; set; }
        public object ReplyToSessionId { get; set; }
        public string TimeToLive { get; set; }
        public object CorrelationId { get; set; }
        public object Subject { get; set; }
        public object To { get; set; }
        public object ContentType { get; set; }
        public object ReplyTo { get; set; }
        public DateTime ScheduledEnqueueTime { get; set; }
        public Applicationproperties ApplicationProperties { get; set; }
        public string LockToken { get; set; }
        public int DeliveryCount { get; set; }
        public DateTime LockedUntil { get; set; }
        public int SequenceNumber { get; set; }
        public object DeadLetterSource { get; set; }
        public int EnqueuedSequenceNumber { get; set; }
        public DateTime EnqueuedTime { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string DeadLetterReason { get; set; }
        public string DeadLetterErrorDescription { get; set; }
        public int State { get; set; }
    }

    public class _Promotedproperties
    {
        public string DiagnosticId { get; set; }
    }

    public class Applicationproperties
    {
        public string DiagnosticId { get; set; }
        public string EntityIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public object SortingParameter { get; set; }
        public string Flow { get; set; }
        public string DeadLetterReason { get; set; }
        public string DeadLetterErrorDescription { get; set; }
    }
}
