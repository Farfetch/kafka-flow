namespace KafkaFlow.Client.Protocol.Messages.Implementations.OffsetFetch
{
    using KafkaFlow.Client.Protocol.Streams;

    public class OffsetFetchV5Response : IResponse
    {
        public int ThrottleTimeMs { get; set; }

        public Topic[] Topics { get; set; }

        public ErrorCode Error { get; set; }

        public void Read(BaseMemoryStream source)
        {
            this.ThrottleTimeMs = source.ReadInt32();
            this.Topics = source.ReadArray<Topic>();
            this.Error = source.ReadErrorCode();
        }

        public class Topic : IResponse
        {
            public string Name { get; set; }

            public Partition[] Partitions { get; set; }

            public void Read(BaseMemoryStream source)
            {
                this.Name = source.ReadString();
                this.Partitions = source.ReadArray<Partition>();
            }
        }

        public class Partition : IResponse
        {
            public int Id { get; set; }

            public long CommittedOffset { get; set; }

            public int CommittedLeaderEpoch { get; set; }

            public string Metadata { get; set; }

            public short ErrorCode { get; set; }

            public void Read(BaseMemoryStream source)
            {
                this.Id = source.ReadInt32();
                this.CommittedOffset = source.ReadInt64();
                this.CommittedLeaderEpoch = source.ReadInt32();
                this.Metadata = source.ReadString();
                this.ErrorCode = source.ReadInt16();
            }
        }
    }
}
