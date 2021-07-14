namespace KafkaFlow.Client.Protocol.Streams
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public abstract class BaseMemoryStream : IReadOnlyList<byte>, IDisposable
    {
        ~BaseMemoryStream()
        {
            this.Dispose();
        }

        protected int length = 0;

        public abstract Span<byte> GetSpan(int size);

        public abstract byte this[int index] { get; }

        public int Length => this.length;

        public abstract int Position { get; set; }

        public IEnumerator<byte> GetEnumerator() => new BaseMemoryStreamEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int Count => this.length;

        private class BaseMemoryStreamEnumerator : IEnumerator<byte>
        {
            private int position = -1;
            private readonly BaseMemoryStream stream;

            public BaseMemoryStreamEnumerator(BaseMemoryStream stream)
            {
                this.stream = stream;
            }

            public bool MoveNext() => ++this.position < this.stream.Length;

            public void Reset() => this.position = -1;

            public byte Current => this.position < this.stream.Length ? this.stream[this.position] : default;

            object IEnumerator.Current => this.Current;

            public void Dispose()
            {
                // Dd nothing
            }
        }

        public abstract void Dispose();
    }
}
