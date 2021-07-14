namespace KafkaFlow.Client.Protocol.Messages.Implementations.ApiVersion
{
    using System;
    using KafkaFlow.Client.Protocol.Streams;

    public class ApiVersionV2Request : IApiVersionRequest
    {
        public ApiKey ApiKey => ApiKey.ApiVersions;

        public short ApiVersion => 2;

        public Type ResponseType => typeof(ApiVersionV2Response);

        public void Write(DynamicMemoryStream destination)
        {
            // Do Nothing
        }
    }
}
