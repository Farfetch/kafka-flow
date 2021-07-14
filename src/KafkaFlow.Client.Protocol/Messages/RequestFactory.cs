namespace KafkaFlow.Client.Protocol.Messages
{
    using KafkaFlow.Client.Protocol.Messages.Implementations.Metadata;
    using KafkaFlow.Client.Protocol.Messages.Implementations.Produce;

    public class RequestFactory : IRequestFactory
    {
        private readonly IBrokerCapabilities capabilities;

        public RequestFactory(IBrokerCapabilities capabilities)
        {
            this.capabilities = capabilities;
        }

        public IProduceRequest CreateProduce(ProduceAcks acks, int timeout)
        {
            // var cap = this.capabilities.GetVersionRange(ApiKey.Produce);

            return new ProduceV8Request(acks, timeout);
        }

        public IMetadataRequest CreateMetadata()
        {
            // var cap = this.capabilities.GetVersionRange(ApiKey.Produce);

            return new MetadataV9Request();
        }
    }
}
