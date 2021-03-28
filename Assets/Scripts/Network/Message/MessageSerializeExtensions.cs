using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public static class MessageSerializeExtensions
{
    public static byte[] Serialize(this object obj)
    {   
        byte[] bytes;
        using (var ms = new MemoryStream())
        {
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);
            bytes = ms.ToArray();
        }
        return bytes;
    }

    public static T Deserialize<T>(this byte[] data)
    {   
        T value;
        using (var ms = new MemoryStream(data))
        {
            IFormatter formatter = new BinaryFormatter();
            value = (T)formatter.Deserialize(ms);    
        }
        return value;
    }
}