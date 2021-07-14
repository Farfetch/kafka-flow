namespace KafkaFlow.Client.Protocol.Streams
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;

    public sealed class StaticMemoryStream : BaseMemoryStream
    {
        private readonly IntPtr buffer;
        private readonly IMemoryManager memoryManager;
        private int position;

        public StaticMemoryStream(IMemoryManager memoryManager, int length)
        {
            this.memoryManager = memoryManager;
            this.buffer = memoryManager.Allocate(length);
            this.length = length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe int Read(Span<byte> buffer)
        {
            var writeCount = Math.Min(this.length - this.position, buffer.Length);

            new Span<byte>((this.buffer + this.position).ToPointer(), writeCount)
                .CopyTo(buffer);

            this.position += writeCount;

            return writeCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void ReadFrom(Stream stream)
        {
            stream.Read(
                new Span<byte>(
                    (this.buffer + this.position).ToPointer(),
                    this.length - this.position));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe Span<byte> GetSpan(int size)
        {
            var start = this.position;
            this.Position += size;
            return new Span<byte>((this.buffer + start).ToPointer(), size);
        }

        public override void Dispose() => this.memoryManager.Free(this.buffer);

        public override unsafe byte this[int index] => *(byte*) (this.buffer + this.position);

        public override int Position
        {
            get => this.position;
            set
            {
                if (value < 0 || value > this.Length)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(this.Position),
                        $"The {nameof(this.Position)} must be greater or equal 0");
                }

                this.position = value;
            }
        }
    }
}
