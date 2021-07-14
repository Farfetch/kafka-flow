namespace KafkaFlow.Client.Protocol.Messages.Implementations.JoinGroup
{
    using KafkaFlow.Client.Protocol.Streams;

    internal class JoinGroupV7Response : ITaggedFields, IJoinGroupResponse
    {
        public int ThrottleTimeMs { get; private set; }

        public ErrorCode Error { get; private set; }

        public int GenerationId { get; private set; }

        public string? ProtocolType { get; private set; }

        public string? ProtocolName { get; private set; }

        public string LeaderId { get; private set; }

        public string MemberId { get; private set; }

        public IJoinGroupResponse.IMember[] Members { get; private set; }

        public TaggedField[] TaggedFields { get; private set; }

        public void Read(BaseMemoryStream source)
        {
            this.ThrottleTimeMs = source.ReadInt32();
            this.Error = (ErrorCode) source.ReadInt16();
            this.GenerationId = source.ReadInt32();
            this.ProtocolType = source.ReadCompactNullableString();
            this.ProtocolName = source.ReadCompactNullableString();
            this.LeaderId = source.ReadCompactString();
            this.MemberId = source.ReadCompactString();
            this.Members = source.ReadCompactArray<Member>();
            this.TaggedFields = source.ReadTaggedFields();
        }

        private class Member : IJoinGroupResponse.IMember, ITaggedFields
        {
            public string MemberId { get; private set; }

            public string? GroupInstanceId { get; private set; }

            public byte[] Metadata { get; private set; }

            public TaggedField[] TaggedFields { get; private set; }

            public void Read(BaseMemoryStream source)
            {
                this.MemberId = source.ReadCompactString();
                this.GroupInstanceId = source.ReadCompactNullableString();
                this.Metadata = source.ReadCompactByteArray();
                this.TaggedFields = source.ReadTaggedFields();
            }
        }
    }
}
