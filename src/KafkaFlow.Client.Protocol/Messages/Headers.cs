namespace KafkaFlow.Client.Protocol.Messages
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using KafkaFlow.Client.Protocol.Streams;

    public class Headers : ISerializable, IEnumerable<KeyValuePair<string, byte[]>>, IRequest, IResponse
    {
        private readonly List<Header> headers = new();

        public void Add(string key, byte[]? value)
        {
            this.headers.Add(new Header(key, value));
        }

        public void Remove(string key)
        {
            this.headers.RemoveAll(x => x.Key == key);
        }

        public IEnumerable<byte[]?> GetValues(string key)
        {
            return this.headers
                .Where(x => x.Key == key)
                .Select(x => x.Value);
        }

        public byte[]? this[string key]
        {
            get => this.GetValues(key).FirstOrDefault();
            set
            {
                this.Remove(key);
                this.Add(key, value);
            }
        }

        public int Count => this.headers.Count;

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            foreach (var (key, value) in this)
            {
                info.AddValue(key, Encoding.UTF8.GetString(value));
            }
        }

        public IEnumerator<KeyValuePair<string, byte[]?>> GetEnumerator()
        {
            return this.headers
                .Select(x => new KeyValuePair<string, byte[]?>(x.Key, x.Value))
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public void Write(DynamicMemoryStream destination)
        {
            destination.WriteVarint(this.headers.Count);

            foreach (var header in this.headers)
            {
                var keyBytes = Encoding.UTF8.GetBytes(header.Key);

                destination.WriteVarint(keyBytes.Length);
                destination.Write(keyBytes);

                if (header.Value is null)
                {
                    destination.WriteVarint(-1);
                }
                else
                {
                    destination.WriteVarint(header.Value.Length);
                    destination.Write(header.Value);
                }
            }
        }

        public void Read(BaseMemoryStream source)
        {
            var count = source.ReadVarint();
            this.headers.Capacity = count;

            for (var i = 0; i < count; ++i)
            {
                var key = source.ReadString(source.ReadVarint());
                var value = source.GetSpan(source.ReadVarint()).ToArray();

                this.Add(key, value);
            }
        }

        private sealed class Header
        {
            public readonly string Key;
            public readonly byte[]? Value;

            public Header(string key, byte[]? value)
            {
                this.Key = key;
                this.Value = value;
            }
        }
    }
}
