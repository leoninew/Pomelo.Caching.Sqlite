using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Pomelo.Caching.Sqlite
{
    class BinarySqliteCacheSerializer : ISqliteCacheSerializer
    {
        private readonly BinaryFormatter _binaryFormatter = new BinaryFormatter();

        public object? DeserializeObject(string value, Type type)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            using var stream = new MemoryStream(bytes);
            return _binaryFormatter.Deserialize(stream);
        }

        public string SerializeObject(object value)
        {
            using var stream = new MemoryStream();
            _binaryFormatter.Serialize(stream, value);
            stream.Seek(0L, SeekOrigin.Begin);
            return Encoding.ASCII.GetString(stream.ToArray());
        }
    }
}