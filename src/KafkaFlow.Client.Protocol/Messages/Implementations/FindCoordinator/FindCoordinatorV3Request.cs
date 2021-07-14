namespace KafkaFlow.Client.Protocol.Messages.Implementations.FindCoordinator
{
    using System;
    using KafkaFlow.Client.Protocol.Streams;

    public class FindCoordinatorV3Request : IRequestMessage<FindCoordinatorV3Response>, ITaggedFields, IFindCoordinatorRequest
    {
        public FindCoordinatorV3Request(string key, byte keyType)
        {
            this.Key = key;
            this.KeyType = keyType;
        }

        public ApiKey ApiKey => ApiKey.FindCoordinator;

        public short ApiVersion => 3;

        public Type ResponseType => typeof(FindCoordinatorV3Response);

        public string Key { get; }

        public byte KeyType { get; }

        public TaggedField[] TaggedFields => Array.Empty<TaggedField>();

        public void Write(DynamicMemoryStream destination)
        {
            destination.WriteCompactString(this.Key);
            destination.WriteByte(this.KeyType);
            destination.WriteTaggedFields(this.TaggedFields);
        }
    }
}
