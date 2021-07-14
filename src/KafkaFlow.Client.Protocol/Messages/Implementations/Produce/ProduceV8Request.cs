namespace KafkaFlow.Client.Protocol.Messages.Implementations.Produce
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KafkaFlow.Client.Protocol.Streams;

    internal class ProduceV8Request : IProduceRequest
    {
        public ProduceV8Request(ProduceAcks acks, int timeout)
        {
            this.Acks = acks;
            this.Timeout = timeout;
        }

        public ApiKey ApiKey => ApiKey.Produce;

        public short ApiVersion => 8;

        public Type ResponseType => typeof(ProduceV8Response);

        public string? TransactionalId = null;

        public ProduceAcks Acks { get; }

        public int Timeout { get; }

        public Dictionary<string, IProduceRequest.ITopic> Topics { get; } = new();

        public IProduceRequest.ITopic CreateTopic(string name) => new Topic(name);

        public void Write(DynamicMemoryStream destination)
        {
            destination.WriteString(this.TransactionalId);
            destination.WriteInt16((short) this.Acks);
            destination.WriteInt32(this.Timeout);
            destination.WriteArray(this.Topics.Select(x => x.Value), this.Topics.Count);
        }

        public class Topic : IProduceRequest.ITopic
        {
            public Topic(string name)
            {
                this.Name = name;
            }

            public string Name { get; }

            public Dictionary<int, IProduceRequest.IPartition> Partitions { get; } =
                new();

            public IProduceRequest.IPartition CreatePartition(int id) => new Partition(id);

            public void Write(DynamicMemoryStream destination)
            {
                destination.WriteString(this.Name);
                destination.WriteArray(this.Partitions.Select(x => x.Value), this.Partitions.Count);
            }
        }

        public class Partition : IProduceRequest.IPartition
        {
            public Partition(int id)
            {
                this.Id = id;
            }

            public int Id { get; }

            public RecordBatch RecordBatch { get; set; } = new();

            public void Write(DynamicMemoryStream destination)
            {
                destination.WriteInt32(this.Id);
                destination.WriteMessage(this.RecordBatch);
            }
        }
    }
}
