using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace EuropeanWars.Core.Data {
    public static class DataConverter {
        public static T FromString<T>(string text) {
            byte[] bytes = System.Convert.FromBase64String(text);

            using (var memStream = new MemoryStream()) {
                var binForm = new BinaryFormatter();
                memStream.Write(bytes, 0, bytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream);
                return (T)obj;
            }
        }

        public static string ToString(object obj) {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream()) {
                bf.Serialize(ms, obj);
                return System.Convert.ToBase64String(ms.ToArray());
            }
        }
    }
}
