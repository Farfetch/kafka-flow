namespace KafkaFlow.Client.Protocol.Messages.Implementations.Heartbeat
{
    using KafkaFlow.Client.Protocol.Streams;

    public class HeartbeatV4Response : IResponse, ITaggedFields, IHeartbeatResponse
    {
        public int ThrottleTimeMs { get; private set; }

        public ErrorCode Error { get; private set; }

        public TaggedField[] TaggedFields { get; private set; }

        public void Read(BaseMemoryStream source)
        {
            this.ThrottleTimeMs = source.ReadInt32();
            this.Error = source.ReadErrorCode();
            this.TaggedFields = source.ReadTaggedFields();
        }
    }
}
