namespace KafkaFlow.Client.Protocol.Messages.Implementations.SyncGroup
{
    using KafkaFlow.Client.Protocol.Streams;

    public class SyncGroupV5Response : IResponse, ITaggedFields
    {
        public int ThrottleTimeMs { get; private set; }

        public ErrorCode Error { get; private set; }

        public string? ProtocolType { get; private set; }

        public string? ProtocolName { get; private set; }

        public byte[] AssignmentMetadata { get; private set; }

        public TaggedField[] TaggedFields { get; private set; }

        public void Read(BaseMemoryStream source)
        {
            this.ThrottleTimeMs = source.ReadInt32();
            this.Error = source.ReadErrorCode();
            this.ProtocolType = source.ReadCompactNullableString();
            this.ProtocolName = source.ReadCompactNullableString();
            this.AssignmentMetadata = source.ReadCompactByteArray();
            this.TaggedFields = source.ReadTaggedFields();
        }
    }
}
