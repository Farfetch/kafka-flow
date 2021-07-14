namespace KafkaFlow.Client.Protocol.Messages.Implementations.ApiVersion
{
    using KafkaFlow.Client.Protocol.Streams;

    public class ApiVersionV2Response : IApiVersionResponse
    {
        public ErrorCode Error { get; private set; }

        public IApiVersionResponse.IApiVersion[] ApiVersions { get; private set; }

        public int ThrottleTime { get; private set; }

        public void Read(BaseMemoryStream source)
        {
            this.Error = source.ReadErrorCode();
            this.ApiVersions = source.ReadArray<ApiVersion>();
            this.ThrottleTime = source.ReadInt32();
        }

        public class ApiVersion : IApiVersionResponse.IApiVersion
        {
            public ApiKey ApiKey { get; private set; }

            public short MinVersion { get; private set; }

            public short MaxVersion { get; private set; }

            public void Read(BaseMemoryStream source)
            {
                this.ApiKey = (ApiKey) source.ReadInt16();
                this.MinVersion = source.ReadInt16();
                this.MaxVersion = source.ReadInt16();
            }
        }
    }
}
