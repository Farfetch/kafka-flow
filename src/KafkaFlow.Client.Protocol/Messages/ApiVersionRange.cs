namespace KafkaFlow.Client.Protocol.Messages
{
    public readonly struct ApiVersionRange
    {
        public ApiKey Api { get; }
        public short Min { get; }
        public short Max { get; }

        public ApiVersionRange(ApiKey api, short min, short max)
        {
            this.Api = api;
            this.Min = min;
            this.Max = max;
        }
    }
}
