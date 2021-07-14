namespace KafkaFlow.Client.Protocol.Messages
{
    public interface IRequestFactory
    {
        IProduceRequest CreateProduce(ProduceAcks acks, int timeout);

        IMetadataRequest CreateMetadata();
    }
}
