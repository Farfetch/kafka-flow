namespace KafkaFlow.Client.Protocol.Messages.Implementations.OffsetCommit
{
    using System;
    using System.Collections.Generic;
    using KafkaFlow.Client.Protocol.Streams;

    public class OffsetCommitV2Request : IOffsetCommitRequest
    {
        public ApiKey ApiKey => ApiKey.OffsetCommit;

        public short ApiVersion => 2;

        public Type ResponseType => typeof(OffsetCommitV2Response);

        public string GroupId { get; set; }

        public int GroupGenerationId { get; set; }

        public string MemberId { get; set; }

        public long RetentionTimeMs { get; set; }

        public List<IOffsetCommitRequest.ITopic> Topics { get; } = new();

        public IOffsetCommitRequest.ITopic AddTopic()
        {
            var topic = new Topic();
            this.Topics.Add(topic);
            return topic;
        }

        public void Write(DynamicMemoryStream destination)
        {
            destination.WriteString(this.GroupId);
            destination.WriteInt32(this.GroupGenerationId);
            destination.WriteString(this.MemberId);
            destination.WriteInt64(this.RetentionTimeMs);
            destination.WriteArray(this.Topics);
        }

        public class Topic : IOffsetCommitRequest.ITopic
        {
            public string Name { get; set; }

            public List<IOffsetCommitRequest.IPartition> Partitions { get; } = new();

            public IOffsetCommitRequest.IPartition AddPartition()
            {
                var partition = new Partition();
                this.Partitions.Add(partition);
                return partition;
            }

            public void Write(DynamicMemoryStream destination)
            {
                destination.WriteString(this.Name);
                destination.WriteArray(this.Partitions);
            }
        }

        public class Partition : IOffsetCommitRequest.IPartition
        {
            public int Id { get; set; }

            public long Offset { get; set; }

            public string Metadata { get; set; }

            public void Write(DynamicMemoryStream destination)
            {
                destination.WriteInt32(this.Id);
                destination.WriteInt64(this.Offset);
                destination.WriteString(this.Metadata);
            }
        }
    }
}
