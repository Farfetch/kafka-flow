namespace KafkaFlow.Client.Protocol.Streams
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;

    public sealed class DynamicMemoryStream : BaseMemoryStream
    {
        private int currentSegment;
        private int relativePosition;

        private readonly IMemoryManager memoryManager;
        private readonly int segmentSize;

        private readonly List<IntPtr> segments = new();

        public DynamicMemoryStream(IMemoryManager memoryManager, int segmentSize)
        {
            this.memoryManager = memoryManager;
            this.segmentSize = segmentSize;
        }

        public DynamicMemoryStream(IMemoryManager memoryManager) : this(memoryManager, 1024)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte[] buffer, int offset, int count) => this.Write(new Span<byte>(buffer, offset, count));

        public unsafe void Write(ReadOnlySpan<byte> buffer)
        {
            var count = buffer.Length;
            var offset = 0;
            var endPosition = this.Position + count;

            this.EnsureCapacity(endPosition);

            var startRelPosition = this.relativePosition;
            var startSegment = this.currentSegment;

            this.Position = endPosition;

            while (count > 0)
            {
                var segment = this.segments[startSegment++];

                var writeCount = Math.Min(this.segmentSize - startRelPosition, count);

                buffer
                    .Slice(offset, writeCount)
                    .CopyTo(new Span<byte>((segment + startRelPosition).ToPointer(), writeCount));

                startRelPosition = 0;

                offset += writeCount;
                count -= writeCount;
            }
        }

        public unsafe void CopyTo(Stream destination)
        {
            var totalBytes = this.length;

            foreach (var segment in this.segments)
            {
                var bytesToWriteCount = Math.Min(totalBytes, this.segmentSize);
                destination.Write(new ReadOnlySpan<byte>(segment.ToPointer(), bytesToWriteCount));

                totalBytes -= bytesToWriteCount;
            }
        }

        public unsafe void CopyTo(DynamicMemoryStream dest)
        {
            var sourceSegmentIndex = this.currentSegment;
            var sourceOffset = this.relativePosition;

            var destSegmentIndex = dest.currentSegment;
            var destOffset = dest.relativePosition;

            var totalWriteCount = this.Length - this.Position;

            dest.EnsureCapacity(dest.Position + totalWriteCount);

            this.Position = this.Length;
            dest.Position += totalWriteCount;

            while (totalWriteCount > 0)
            {
                var sourceSegment = this.segments[sourceSegmentIndex];
                var destSegment = dest.segments[destSegmentIndex];

                var sourceWriteCount = Math.Min(this.segmentSize - sourceOffset, totalWriteCount);
                var destWriteCount = Math.Min(dest.segmentSize - destOffset, totalWriteCount);
                var writeCount = Math.Min(sourceWriteCount, destWriteCount);

                Buffer.MemoryCopy(
                    (sourceSegment + sourceOffset).ToPointer(),
                    (destSegment + destOffset).ToPointer(),
                    writeCount,
                    writeCount);

                totalWriteCount -= writeCount;

                if (dest.segmentSize == writeCount + destOffset)
                {
                    destOffset = 0;
                    ++destSegmentIndex;
                }
                else
                {
                    destOffset += writeCount;
                }

                if (this.segmentSize == writeCount + sourceOffset)
                {
                    sourceOffset = 0;
                    ++sourceSegmentIndex;
                }
                else
                {
                    sourceOffset += writeCount;
                }
            }
        }

        // public unsafe Span<byte> GetSpan(int size)
        // {
        //     this.EnsureCapacity(this.Position + size);
        //
        //     if (this.relativePosition + size <= this.segmentSize)
        //     {
        //         return new Span<byte>(this.segments[this.currentSegment].ToPointer(), size);
        //     }
        //     
        //     
        //     new Span<byte>(). + new Span<byte>()
        // }

        public override Span<byte> GetSpan(int size)
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            foreach (var segment in this.segments)
            {
                this.memoryManager.Free(segment);
            }

            this.segments.Clear();
        }

        private void EnsureCapacity(long capacity)
        {
            if (capacity <= this.Capacity)
                return;

            var diff = capacity - this.Capacity;

            var newSegmentsCount = (int) Math.Ceiling(diff / (double) this.segmentSize);
            this.Capacity += newSegmentsCount * this.segmentSize;

            while (--newSegmentsCount >= 0)
            {
                this.segments.Add(this.memoryManager.Allocate(this.segmentSize));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetSegment(long globalPosition) => (int) Math.Floor(globalPosition / (double) this.segmentSize);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetRelativePosition(long globalPosition) => (int) (globalPosition % this.segmentSize);

        public override unsafe byte this[int index] => *(byte*) (this.segments[this.GetSegment(index)] + this.GetRelativePosition(index));

        public override int Position
        {
            get => this.segmentSize * this.currentSegment + this.relativePosition;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(this.Position),
                        $"The {nameof(this.Position)} must be greater or equal 0");
                }

                if (value > this.length)
                    this.length = value;

                this.currentSegment = this.GetSegment(value);
                this.relativePosition = this.GetRelativePosition(value);
            }
        }

        public int Capacity { get; private set; }

        public void WriteByte(byte value) => this.Write(new[] { value }, 0, 1);
    }
}
