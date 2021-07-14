namespace KafkaFlow.Client.Protocol.Messages.Implementations.Produce
{
    using KafkaFlow.Client.Protocol.Streams;

    public class ProduceV8Response : IProduceResponse
    {
        public IProduceResponse.ITopic[] Topics { get; private set; }

        public int ThrottleTimeMs { get; private set; }

        public void Read(BaseMemoryStream source)
        {
            this.Topics = source.ReadArray<Topic>();
            this.ThrottleTimeMs = source.ReadInt32();
        }

        public class Topic : IProduceResponse.ITopic
        {
            public string Name { get; private set; }

            public IProduceResponse.IPartition[] Partitions { get; private set; }

            public void Read(BaseMemoryStream source)
            {
                this.Name = source.ReadString();
                this.Partitions = source.ReadArray<Partition>();
            }
        }

        public class Partition : IProduceResponse.IPartition
        {
            public int Id { get; private set; }

            public ErrorCode Error { get; private set; }

            public long BaseOffset { get; private set; }

            public long LogAppendTime { get; private set; }

            public long LogStartOffset { get; private set; }

            public IProduceResponse.IRecordError[] RecordErrors { get; private set; }

            public string? ErrorMessage { get; private set; }

            public void Read(BaseMemoryStream source)
            {
                this.Id = source.ReadInt32();
                this.Error = source.ReadErrorCode();
                this.BaseOffset = source.ReadInt64();
                this.LogAppendTime = source.ReadInt64();
                this.LogStartOffset = source.ReadInt64();
                this.RecordErrors = source.ReadArray<RecordError>();
                this.ErrorMessage = source.ReadNullableString();
            }
        }

        public class RecordError : IProduceResponse.IRecordError
        {
            public int BatchIndex { get; private set; }

            public string? Message { get; private set; }

            public void Read(BaseMemoryStream source)
            {
                this.BatchIndex = source.ReadInt32();
                this.Message = source.ReadNullableString();
            }
        }
    }
}
