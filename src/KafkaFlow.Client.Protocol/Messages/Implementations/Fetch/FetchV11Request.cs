namespace KafkaFlow.Client.Protocol.Messages.Implementations.Fetch
{
    using System;
    using KafkaFlow.Client.Protocol.Streams;

    public class FetchV11Request : IRequestMessage<FetchV11Response>
    {
        public ApiKey ApiKey => ApiKey.Fetch;

        public short ApiVersion => 11;

        public int ReplicaId { get; set; }

        public int MaxWaitTime { get; set; }

        public int MinBytes { get; set; }

        public int MaxBytes { get; set; }

        public byte IsolationLevel { get; set; }

        public int SessionId { get; set; }

        public int SessionEpoch { get; set; }

        public Topic[] Topics { get; set; } = Array.Empty<Topic>();

        public ForgottenTopic[] ForgottenTopics { get; set; } = Array.Empty<ForgottenTopic>();

        public string RackId { get; set; } = string.Empty;

        public void Write(DynamicMemoryStream destination)
        {
            destination.WriteInt32(this.ReplicaId);
            destination.WriteInt32(this.MaxWaitTime);
            destination.WriteInt32(this.MinBytes);
            destination.WriteInt32(this.MaxBytes);
            destination.WriteByte(this.IsolationLevel);
            destination.WriteInt32(this.SessionId);
            destination.WriteInt32(this.SessionEpoch);
            destination.WriteArray(this.Topics);
            destination.WriteArray(this.ForgottenTopics);
            destination.WriteString(this.RackId);
        }

        public class Topic : IRequest
        {
            public string Name { get; set; }

            public Partition[] Partitions { get; set; } = Array.Empty<Partition>();

            public void Write(DynamicMemoryStream destination)
            {
                destination.WriteString(this.Name);
                destination.WriteArray(this.Partitions);
            }
        }

        public class Partition : IRequest
        {
            public int Id { get; set; }

            public int CurrentLeaderEpoch { get; set; }

            public long FetchOffset { get; set; }

            public long LogStartOffset { get; set; }

            public int PartitionMaxBytes { get; set; }

            public void Write(DynamicMemoryStream destination)
            {
                destination.WriteInt32(this.Id);
                destination.WriteInt32(this.CurrentLeaderEpoch);
                destination.WriteInt64(this.FetchOffset);
                destination.WriteInt64(this.LogStartOffset);
                destination.WriteInt32(this.PartitionMaxBytes);
            }
        }

        public class ForgottenTopic : IRequest
        {
            public string Name { get; set; }

            public int[] Partitions { get; set; }

            public void Write(DynamicMemoryStream destination)
            {
                destination.WriteString(this.Name);
                destination.WriteInt32Array(this.Partitions);
            }
        }

        public Type ResponseType { get; }
    }
}
