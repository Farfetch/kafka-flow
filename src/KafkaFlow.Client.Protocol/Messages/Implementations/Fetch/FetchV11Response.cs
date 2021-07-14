namespace KafkaFlow.Client.Protocol.Messages.Implementations.Fetch
{
    using KafkaFlow.Client.Protocol.Streams;

    public class FetchV11Response : IResponse
    {
        public int ThrottleTimeMs { get; set; }

        public ErrorCode Error { get; set; }

        public int SessionId { get; set; }

        public Topic[] Topics { get; set; }

        public void Read(BaseMemoryStream source)
        {
            this.ThrottleTimeMs = source.ReadInt32();
            this.Error = source.ReadErrorCode();
            this.SessionId = source.ReadInt32();
            this.Topics = source.ReadArray<Topic>();
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
            public PartitionHeader Header { get; set; }

            public RecordBatch RecordBatch { get; set; }

            public void Read(BaseMemoryStream source)
            {
                this.Header = source.ReadMessage<PartitionHeader>();
                this.RecordBatch = source.ReadMessage<RecordBatch>();
            }
        }

        public class PartitionHeader : IResponse
        {
            public int Id { get; set; }

            public ErrorCode Error { get; set; }

            public long HighWatermark { get; set; }

            public long LastStableOffset { get; set; }

            public long LogStartOffset { get; set; }

            public AbortedTransaction[] AbortedTransactions { get; set; }

            public int PreferredReadReplica { get; set; }

            public void Read(BaseMemoryStream source)
            {
                this.Id = source.ReadInt32();
                this.Error = source.ReadErrorCode();
                this.HighWatermark = source.ReadInt64();
                this.LastStableOffset = source.ReadInt64();
                this.LogStartOffset = source.ReadInt64();
                this.AbortedTransactions = source.ReadArray<AbortedTransaction>();
                this.PreferredReadReplica = source.ReadInt32();
            }
        }

        public class AbortedTransaction : IResponse
        {
            public long ProducerId { get; set; }

            public long FirstOffset { get; set; }

            public void Read(BaseMemoryStream source)
            {
                this.ProducerId = source.ReadInt64();
                this.FirstOffset = source.ReadInt64();
            }
        }
    }
}
