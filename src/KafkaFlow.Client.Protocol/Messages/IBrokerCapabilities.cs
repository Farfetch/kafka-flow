namespace KafkaFlow.Client.Protocol.Messages
{
    public interface IBrokerCapabilities
    {
        ApiVersionRange GetVersionRange(ApiKey api);
    }
}
