namespace KafkaFlow.Client.Protocol.Streams
{
    using System;
    using System.Buffers.Binary;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;
    using KafkaFlow.Client.Protocol.Messages;

    public static class StreamWriteExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteMessage(this DynamicMemoryStream destination, IRequest message) => message.Write(destination);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt16(this DynamicMemoryStream destination, short value)
        {
            Span<byte> tmp = stackalloc byte[2];
            BinaryPrimitives.WriteInt16BigEndian(tmp, value);
            destination.Write(tmp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt32(this DynamicMemoryStream destination, int value)
        {
            Span<byte> tmp = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(tmp, value);
            destination.Write(tmp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt64(this DynamicMemoryStream destination, long value)
        {
            Span<byte> tmp = stackalloc byte[8];
            BinaryPrimitives.WriteInt64BigEndian(tmp, value);
            destination.Write(tmp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt32Array(this DynamicMemoryStream destination, IReadOnlyList<int> values)
        {
            destination.WriteInt32(values.Count);
            InternalWriteInt32Array(destination, values);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteCompactInt32Array(this DynamicMemoryStream destination, IReadOnlyList<int> values)
        {
            destination.WriteUVarint((uint) values.Count);
            InternalWriteInt32Array(destination, values);
        }

        private static void InternalWriteInt32Array(DynamicMemoryStream destination, IReadOnlyList<int> values)
        {
            for (var i = 0; i < values.Count; ++i)
            {
                destination.WriteInt32(values[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteString(this DynamicMemoryStream destination, string? value)
        {
            if (value is null)
            {
                destination.WriteInt16(-1);
                return;
            }

            destination.WriteInt16(Convert.ToInt16(value.Length));
            destination.Write(Encoding.UTF8.GetBytes(value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteCompactString(this DynamicMemoryStream destination, string value)
        {
            destination.WriteUVarint((uint) value.Length + 1u);
            destination.Write(Encoding.UTF8.GetBytes(value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteCompactNullableString(this DynamicMemoryStream destination, string? value)
        {
            if (value is null)
            {
                destination.WriteUVarint(0);
                return;
            }

            destination.WriteCompactString(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteCompactNullableByteArray(this DynamicMemoryStream destination, byte[]? data)
        {
            if (data is null)
            {
                destination.WriteUVarint(0);
                return;
            }

            destination.WriteCompactByteArray(data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteCompactByteArray(this DynamicMemoryStream destination, byte[] data)
        {
            destination.WriteUVarint((uint) data.Length + 1u);
            destination.Write(data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBoolean(this DynamicMemoryStream destination, bool value)
        {
            destination.WriteByte((byte) (value ? 1 : 0));
        }

        public static void WriteArray<TMessage>(this DynamicMemoryStream destination, IEnumerable<TMessage> items, int count)
            where TMessage : IRequest
        {
            destination.WriteInt32(count);

            foreach (var item in items)
            {
                destination.WriteMessage(item);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteArray<TMessage>(this DynamicMemoryStream destination, IReadOnlyCollection<TMessage> items)
            where TMessage : IRequest
        {
            destination.WriteArray(items, items.Count);
        }

        public static void WriteTaggedFields(this DynamicMemoryStream destination, IReadOnlyList<TaggedField> items)
        {
            destination.WriteUVarint((uint) items.Count);

            for (var i = 0; i < items.Count; ++i)
            {
                destination.WriteMessage(items[i]);
            }
        }

        public static void WriteCompactArray<TMessage>(this DynamicMemoryStream destination, IReadOnlyList<TMessage> items)
            where TMessage : IRequest
        {
            destination.WriteUVarint((uint) items.Count + 1);

            for (var i = 0; i < items.Count; ++i)
            {
                destination.WriteMessage(items[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WriteVarint(this DynamicMemoryStream destination, long num) =>
            destination.WriteUVarint(((ulong) num << 1) ^ ((ulong) num >> 63));

        public static int WriteUVarint(this DynamicMemoryStream destination, ulong num)
        {
            const ulong endMask = 0b1000_0000;
            const ulong valueMask = 0b0111_1111;

            var bytesWritten = 0;

            do
            {
                var value = (byte) ((num & valueMask) | (num > valueMask ? endMask : 0));
                destination.WriteByte(value);
                ++bytesWritten;
                num >>= 7;
            } while (num != 0);

            return bytesWritten;
        }
    }
}
