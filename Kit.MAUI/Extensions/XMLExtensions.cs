using Newtonsoft.Json;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Kit
{
    public static class XMLExtensions
    {
        public static object XmlDeserializeFromString(this string objectData, Type type)
        {
            var serializer = new XmlSerializer(type);
            object result;

            using (TextReader reader = new StringReader(objectData))
            {
                result = serializer.Deserialize(reader);
            }

            return result;
        }

        public static T XmlDeserializeFromString<T>(this string objectData)
        {
            return (T)XmlDeserializeFromString(objectData, typeof(T));
        }

        public static string SerializeObject<T>(this T toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, toSerialize);
                return textWriter.ToString();
            }
        }

        public static string JsonSerializeObject<T>(this T toSerialize)
        {
            JsonSerializer xmlSerializer = JsonSerializer.Create();
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    xmlSerializer.Serialize(jsonWriter: writer, value: toSerialize, typeof(T));
                }
            }
            return sb.ToString();
        }

        public static T ConvertNode<T>(this XmlNode node) where T : class
        {
            MemoryStream stm = new MemoryStream();

            StreamWriter stw = new StreamWriter(stm);
            stw.Write(node.OuterXml);
            stw.Flush();

            stm.Position = 0;

            XmlSerializer ser = new XmlSerializer(typeof(T));
            T result = (ser.Deserialize(stm) as T);

            return result;
        }
    }
}