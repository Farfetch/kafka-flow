﻿namespace KafkaFlow.Sample.Avro.Handlers
{
    using System;
    using System.Threading.Tasks;
    using KafkaFlow.TypedHandler;
    using MessageTypes;

    public class AvroMessageHandler2 : IMessageHandler<LogMessages2>
    {
        public Task Handle(IMessageContext context, LogMessages2 message)
        {
            Console.WriteLine(
                "Partition: {0} | Offset: {1} | Message: {2}",
                context.ConsumerContext.Partition,
                context.ConsumerContext.Offset,
                message.Message);

            return Task.CompletedTask;
        }
    }
}
