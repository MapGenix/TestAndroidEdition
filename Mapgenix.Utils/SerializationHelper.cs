using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Mapgenix.Utils
{
    public static class SerializationHelper
    {
       
        public static T CloneDeep<T>(T instance)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, instance);
            stream.Seek(0, SeekOrigin.Begin);
            return (T)formatter.Deserialize(stream);
        }
    }
    
}
