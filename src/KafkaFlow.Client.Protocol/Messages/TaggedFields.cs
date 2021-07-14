namespace KafkaFlow.Client.Protocol.Messages
{
    using System;
    using KafkaFlow.Client.Protocol.Streams;

    public class TaggedField : IRequest, IResponse
    {
        public int Tag { get; private set; }

        public byte[] Data { get; private set; } = Array.Empty<byte>();

        public void Write(DynamicMemoryStream destination)
        {
            destination.WriteUVarint((ulong) this.Tag);
            destination.WriteUVarint((ulong) this.Data.Length);
            destination.Write(this.Data);
        }

        public void Read(BaseMemoryStream source)
        {
            this.Tag = source.ReadUVarint();
            this.Data = source.GetSpan(source.ReadUVarint()).ToArray();
        }
    }
}
