namespace KafkaFlow.Client.Protocol
{
    using System;
    using System.Diagnostics;
    using KafkaFlow.Client.Protocol.Messages;
    using KafkaFlow.Client.Protocol.Streams;

    public class Request : IRequest
    {
        public int CorrelationId { get; }

        public string ClientId { get; }

        public TaggedField[] TaggedFields => Array.Empty<TaggedField>();

        public IRequestMessage Message { get; }

        public Request(
            int correlationId,
            string clientId,
            IRequestMessage message)
        {
            this.CorrelationId = correlationId;
            this.ClientId = clientId;
            this.Message = message;
        }

        public void Write(DynamicMemoryStream destination)
        {
            destination.WriteInt32(0); // Skip message size to write later
            var startPosition = destination.Position;
            destination.WriteInt16((short) this.Message.ApiKey);
            destination.WriteInt16(this.Message.ApiVersion);
            destination.WriteInt32(this.CorrelationId);
            destination.WriteString(this.ClientId);

            if (this.Message is ITaggedFields)
                destination.WriteTaggedFields(this.TaggedFields);

            destination.WriteMessage(this.Message);

            var endPosition = destination.Position;

            // Write message size at the beginning
            destination.Position = 0;
            destination.WriteInt32(Convert.ToInt32(endPosition - startPosition));

            Debug.WriteLine($"Sending {this.Message.GetType().Name} with {destination.Length:N0}b");
        }
    }
}
