namespace KafkaFlow.Client.Protocol.Messages
{
    using System.Collections.Generic;

    public interface IProduceRequest : IRequestMessage<IProduceResponse>
    {
        ProduceAcks Acks { get; }

        int Timeout { get; }

        Dictionary<string, ITopic> Topics { get; }

        ITopic CreateTopic(string name);

        public interface ITopic : IRequest
        {
            string Name { get; }

            Dictionary<int, IPartition> Partitions { get; }

            IPartition CreatePartition(int id);
        }

        public interface IPartition : IRequest
        {
            int Id { get; }

            RecordBatch RecordBatch { get; set; }
        }
    }
}
