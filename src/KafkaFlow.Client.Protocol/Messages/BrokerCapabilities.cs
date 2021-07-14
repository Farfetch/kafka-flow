namespace KafkaFlow.Client.Protocol.Messages
{
    using System.Collections.Generic;
    using System.Linq;

    public class BrokerCapabilities : IBrokerCapabilities
    {
        private readonly Dictionary<ApiKey, ApiVersionRange> ranges;

        public BrokerCapabilities(IEnumerable<ApiVersionRange> ranges)
        {
            this.ranges = ranges.ToDictionary(x => x.Api);
        }

        public ApiVersionRange GetVersionRange(ApiKey api)
        {
            return this.ranges[api];
        }
    }
}
