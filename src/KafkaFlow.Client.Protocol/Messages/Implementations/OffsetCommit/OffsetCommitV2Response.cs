namespace KafkaFlow.Client.Protocol.Messages.Implementations.OffsetCommit
{
    using KafkaFlow.Client.Protocol.Streams;

    internal class OffsetCommitV2Response : IResponse, IOffsetCommitResponse
    {
        public IOffsetCommitResponse.ITopic[] Topics { get; private set; }

        public void Read(BaseMemoryStream source)
        {
            this.Topics = source.ReadArray<Topic>();
        }

        private class Topic : IOffsetCommitResponse.ITopic
        {
            public string Name { get; private set; }

            public IOffsetCommitResponse.IPartition[] Partitions { get; private set; }

            public void Read(BaseMemoryStream source)
            {
                this.Name = source.ReadString();
                this.Partitions = source.ReadArray<Partition>();
            }
        }

        private class Partition : IOffsetCommitResponse.IPartition
        {
            public int Id { get; private set; }

            public ErrorCode Error { get; private set; }

            public void Read(BaseMemoryStream source)
            {
                this.Id = source.ReadInt32();
                this.Error = source.ReadErrorCode();
            }
        }
    }
}
