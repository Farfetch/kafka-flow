namespace KafkaFlow.Client.Protocol.Messages.Implementations.OffsetFetch
{
    using System;
    using KafkaFlow.Client.Protocol.Streams;

    public class OffsetFetchV5Request : IRequestMessage<OffsetFetchV5Response>
    {
        public OffsetFetchV5Request(string groupId, Topic[] topics)
        {
            this.GroupId = groupId;
            this.Topics = topics;
        }

        public ApiKey ApiKey => ApiKey.OffsetFetch;

        public short ApiVersion => 5;

        public Type ResponseType => typeof(OffsetFetchV5Response);

        public string GroupId { get; }

        public Topic[] Topics { get; }

        public void Write(DynamicMemoryStream destination)
        {
            destination.WriteString(this.GroupId);
            destination.WriteArray(this.Topics);
        }

        public class Topic : IRequest
        {
            public Topic(string name, int[] partitions)
            {
                this.Name = name;
                this.Partitions = partitions;
            }

            public string Name { get; }

            public int[] Partitions { get; }

            public void Write(DynamicMemoryStream destination)
            {
                destination.WriteString(this.Name);
                destination.WriteInt32Array(this.Partitions);
            }
        }
    }
}
