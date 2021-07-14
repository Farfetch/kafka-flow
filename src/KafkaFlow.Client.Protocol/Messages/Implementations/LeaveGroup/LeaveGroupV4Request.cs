namespace KafkaFlow.Client.Protocol.Messages.Implementations.LeaveGroup
{
    using System;
    using System.Collections.Generic;
    using KafkaFlow.Client.Protocol.Streams;

    internal class LeaveGroupV4Request : ITaggedFields, ILeaveGroupRequest
    {
        public ApiKey ApiKey => ApiKey.LeaveGroup;

        public short ApiVersion => 4;

        public Type ResponseType => typeof(LeaveGroupV4Response);

        public string GroupId { get; set; }

        public List<ILeaveGroupRequest.IMember> Members { get; set; }

        public TaggedField[] TaggedFields { get; set; } = Array.Empty<TaggedField>();

        public ILeaveGroupRequest.IMember CreateMember() => new Member();

        public void Write(DynamicMemoryStream destination)
        {
            destination.WriteCompactString(this.GroupId);
            destination.WriteCompactArray(this.Members);
            destination.WriteTaggedFields(this.TaggedFields);
        }

        private class Member : ITaggedFields, ILeaveGroupRequest.IMember
        {
            public string MemberId { get; set; }

            public string? GroupInstanceId { get; set; }

            public TaggedField[] TaggedFields { get; set; } = Array.Empty<TaggedField>();

            public void Write(DynamicMemoryStream destination)
            {
                destination.WriteCompactString(this.MemberId);
                destination.WriteCompactNullableString(this.GroupInstanceId);
                destination.WriteTaggedFields(this.TaggedFields);
            }
        }
    }
}
