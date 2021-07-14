namespace KafkaFlow.Client.Protocol.Messages.Implementations.JoinGroup
{
    using System;
    using KafkaFlow.Client.Protocol.Streams;

    internal class JoinGroupV7Request : ITaggedFields, IJoinGroupRequest
    {
        public ApiKey ApiKey => ApiKey.JoinGroup;

        public short ApiVersion => 7;

        public Type ResponseType => typeof(JoinGroupV7Response);

        public string GroupId { get; set; }

        public int SessionTimeoutMs { get; set; }

        public int RebalanceTimeoutMs { get; set; }

        public string MemberId { get; set; }

        public string? GroupInstanceId { get; set; }

        public string ProtocolType { get; set; }

        public IJoinGroupRequest.IProtocol[] SupportedProtocols { get; set; }

        public TaggedField[] TaggedFields { get; set; } = Array.Empty<TaggedField>();

        public IJoinGroupRequest.IProtocol CreateProtocol() => new Protocol();

        public void Write(DynamicMemoryStream destination)
        {
            destination.WriteCompactString(this.GroupId);
            destination.WriteInt32(this.SessionTimeoutMs);
            destination.WriteInt32(this.RebalanceTimeoutMs);
            destination.WriteCompactString(this.MemberId);
            destination.WriteCompactNullableString(this.GroupInstanceId);
            destination.WriteCompactString(this.ProtocolType);
            destination.WriteCompactArray(this.SupportedProtocols);
            destination.WriteTaggedFields(this.TaggedFields);
        }

        private class Protocol : IJoinGroupRequest.IProtocol, ITaggedFields
        {
            public string Name { get; set; }

            public byte[] Metadata { get; set; }

            public TaggedField[] TaggedFields { get; set; } = Array.Empty<TaggedField>();

            public void Write(DynamicMemoryStream destination)
            {
                destination.WriteCompactString(this.Name);
                destination.WriteCompactByteArray(this.Metadata);
                destination.WriteTaggedFields(this.TaggedFields);
            }
        }
    }
}
