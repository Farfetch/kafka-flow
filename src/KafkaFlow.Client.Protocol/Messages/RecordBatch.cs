namespace KafkaFlow.Client.Protocol.Messages
{
    using System;
    using System.Collections.Generic;
    using KafkaFlow.Client.Protocol.Streams;

    public class RecordBatch : IRequest, IResponse
    {
        private readonly LinkedList<Record> records = new();

        public long BaseOffset { get; private set; }

        public int BatchLength { get; private set; }

        public int PartitionLeaderEpoch { get; private set; }

        public byte Magic { get; private set; } = 2;

        public int Crc { get; private set; }

        public short Attributes { get; private set; }

        public int LastOffsetDelta { get; private set; }

        public long FirstTimestamp { get; private set; }

        public long MaxTimestamp { get; private set; }

        public long ProducerId { get; private set; } = -1;

        public short ProducerEpoch { get; private set; } = -1;

        public int BaseSequence { get; private set; } = -1;

        public IReadOnlyCollection<Record> Records => this.records;

        public void AddRecord(Record record)
        {
            // lock (this.records)
            // {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (this.records.Count == 0)
            {
                this.FirstTimestamp = now;
                record.OffsetDelta = 0;
                record.TimestampDelta = 0;
            }
            else
            {
                record.TimestampDelta = (int) (now - this.FirstTimestamp);
                record.OffsetDelta = this.LastOffsetDelta = this.records.Count;
            }

            this.MaxTimestamp = now;
            this.records.AddLast(record);
            // }
        }

        public void Write(DynamicMemoryStream destination)
        {
            // destination.WriteInt32(crcSliceLength + 8 + 4 + 4 + 1 + 4);
            var lengthStartPosition = destination.Position;
            destination.WriteInt32(0); // Length will be filled in the end
            destination.WriteInt64(this.BaseOffset);
            destination.WriteInt32(0); // BatchLength will be filled in the end
            destination.WriteInt32(this.PartitionLeaderEpoch);
            destination.WriteByte(this.Magic);
            destination.WriteInt32(0); // CRC will be filled in the end

            var crcPosition = destination.Position;
            destination.WriteInt16(this.Attributes);
            destination.WriteInt32(this.LastOffsetDelta);
            destination.WriteInt64(this.FirstTimestamp);
            destination.WriteInt64(this.MaxTimestamp);
            destination.WriteInt64(this.ProducerId);
            destination.WriteInt16(this.ProducerEpoch);
            destination.WriteInt32(this.BaseSequence);
            destination.WriteArray(this.records);
            var endPosition = destination.Position;

            // Write total length
            var totalLength = endPosition - lengthStartPosition - 4;
            destination.Position = lengthStartPosition;
            destination.WriteInt32(totalLength);

            // Write batch length
            destination.Position = lengthStartPosition + 4 + 8;
            destination.WriteInt32(this.BatchLength = totalLength - 8 - 4);

            // Write CRC
            var crc = Crc32CHash.Compute(destination, crcPosition, endPosition - crcPosition);
            destination.Position = crcPosition - 4;
            destination.WriteInt32((int) crc);

            destination.Position = endPosition;
        }

        public void Read(BaseMemoryStream source)
        {
            var size = source.ReadInt32();

            if (size == 0)
                return;

            this.BaseOffset = source.ReadInt64();
            this.BatchLength = source.ReadInt32();
            this.PartitionLeaderEpoch = source.ReadInt32();
            this.Magic = source.ReadByte();
            this.Crc = source.ReadInt32();
            this.Attributes = source.ReadInt16();
            this.LastOffsetDelta = source.ReadInt32();
            this.FirstTimestamp = source.ReadInt64();
            this.MaxTimestamp = source.ReadInt64();
            this.ProducerId = source.ReadInt64();
            this.ProducerEpoch = source.ReadInt16();
            this.BaseSequence = source.ReadInt32();

            var totalRecords = source.ReadInt32();

            for (var i = 0; i < totalRecords; i++)
            {
                this.records.AddLast(source.ReadMessage<Record>());
            }
        }

        public class Record : IRequest, IResponse
        {
            public int Length { get; private set; }

            public byte Attributes { get; private set; }

            public int TimestampDelta { get; internal set; }

            public int OffsetDelta { get; internal set; }

            public byte[]? Key { get; set; }

            public byte[]? Value { get; set; }

            public Headers? Headers { get; set; }

            public void Write(DynamicMemoryStream destination)
            {
                using var tmp = new DynamicMemoryStream(MemoryManager.Instance, 256);

                tmp.WriteByte(this.Attributes);
                tmp.WriteVarint(this.TimestampDelta);
                tmp.WriteVarint(this.OffsetDelta);

                if (this.Key is null)
                {
                    tmp.WriteVarint(-1);
                }
                else
                {
                    tmp.WriteVarint(this.Key.Length);
                    tmp.Write(this.Key);
                }

                if (this.Value is null)
                {
                    tmp.WriteVarint(-1);
                }
                else
                {
                    tmp.WriteVarint(this.Value.Length);
                    tmp.Write(this.Value);
                }

                if (this.Headers is null)
                {
                    tmp.WriteVarint(0);
                }
                else
                {
                    tmp.WriteMessage(this.Headers);
                }

                destination.WriteVarint(this.Length = Convert.ToInt32(tmp.Length));

                tmp.Position = 0;
                tmp.CopyTo(destination);
            }

            public void Read(BaseMemoryStream source)
            {
                this.Length = source.ReadVarint();
                this.Attributes = (byte) source.ReadByte();
                this.TimestampDelta = source.ReadVarint();
                this.OffsetDelta = source.ReadVarint();
                this.Key = source.GetSpan(source.ReadVarint()).ToArray();
                this.Value = source.GetSpan(source.ReadVarint()).ToArray();
                this.Headers = source.ReadMessage<Headers>();
            }
        }
    }
}
