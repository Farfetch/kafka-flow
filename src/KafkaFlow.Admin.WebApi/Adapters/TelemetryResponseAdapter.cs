namespace KafkaFlow.Admin.WebApi.Adapters
{
    using System.Collections.Generic;
    using System.Linq;
    using KafkaFlow.Admin.Messages;
    using KafkaFlow.Admin.WebApi.Contracts;

    internal static class TelemetryResponseAdapter
    {
        internal static TelemetryResponse Adapt(this IEnumerable<ConsumerTelemetryMetric> metrics)
        {
            return new TelemetryResponse
            {
                Groups = metrics
                    .GroupBy(metric => metric.GroupId)
                    .Select(
                        groupedMetric => new TelemetryResponse.ConsumerGroup
                        {
                            GroupId = groupedMetric.First().GroupId,
                            Consumers = groupedMetric
                                .Select(x => x)
                                .GroupBy(x => x.ConsumerName)
                                .Select(
                                    metric => new TelemetryResponse.Consumer
                                    {
                                        Name = metric.First().ConsumerName,
                                        WorkersCount = metric.OrderByDescending(x=> x.SentAt).First().WorkersCount,
                                        Assignments = metric.Select(
                                            m => new TelemetryResponse.TopicPartitionAssignment
                                            {
                                                InstanceName = m.InstanceName,
                                                TopicName = m.Topic,
                                                Status = m.Status.ToString(),
                                                LastUpdate = m.SentAt,
                                                PausedPartitions = m.PausedPartitions,
                                                RunningPartitions = m.RunningPartitions,
                                                Lag = m.Lag,
                                            }),
                                    }),
                        }),
            };
        }
    }
}
