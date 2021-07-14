namespace KafkaFlow.Client.Protocol.Messages.Implementations.Metadata
{
    using System;
    using KafkaFlow.Client.Protocol.Streams;

    internal class MetadataV9Request : IMetadataRequest, ITaggedFields
    {
        public ApiKey ApiKey => ApiKey.Metadata;

        public short ApiVersion => 9;

        public Type ResponseType => typeof(MetadataV9Response);

        public IMetadataRequest.ITopic[] Topics { get; set; } = Array.Empty<IMetadataRequest.ITopic>();

        public bool AllowAutoTopicCreation { get; set; }

        public bool IncludeClusterAuthorizedOperations { get; set; }

        public bool IncludeTopicAuthorizedOperations { get; set; }

        public TaggedField[] TaggedFields { get; } = Array.Empty<TaggedField>();

        public IMetadataRequest.ITopic CreateTopic() => new Topic();

        public void Write(DynamicMemoryStream destination)
        {
            destination.WriteCompactArray(this.Topics);
            destination.WriteBoolean(this.AllowAutoTopicCreation);
            destination.WriteBoolean(this.IncludeClusterAuthorizedOperations);
            destination.WriteBoolean(this.IncludeTopicAuthorizedOperations);
            destination.WriteTaggedFields(this.TaggedFields);
        }

        private class Topic : IMetadataRequest.ITopic, ITaggedFields
        {
            public string Name { get; set; } = string.Empty;

            public TaggedField[] TaggedFields { get; } = Array.Empty<TaggedField>();

            public void Write(DynamicMemoryStream destination)
            {
                destination.WriteCompactString(this.Name);
                destination.WriteTaggedFields(this.TaggedFields);
            }
        }
    }
}
