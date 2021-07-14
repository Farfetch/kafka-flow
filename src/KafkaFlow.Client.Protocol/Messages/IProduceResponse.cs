namespace KafkaFlow.Client.Protocol.Messages
{
    public interface IProduceResponse : IResponse
    {
        ITopic[] Topics { get; }

        public interface ITopic : IResponse
        {
            string Name { get; }

            IPartition[] Partitions { get; }
        }

        public interface IPartition : IResponse
        {
            int Id { get; }

            ErrorCode Error { get; }

            long BaseOffset { get; }

            IRecordError[] RecordErrors { get; }

            string? ErrorMessage { get; }
        }

        public interface IRecordError : IResponse
        {
            int BatchIndex { get; }

            string? Message { get; }
        }
    }
}
