using System.IO;
using System.Xml.Linq;

namespace Library.Xml
{
    public static class XmlWorker
    {
        public static XElement LoadXmlFile(string path)
        {
            var result = Path.Combine(Directory.GetCurrentDirectory(), path);
            if (!File.Exists(result))
            {
                throw new FileNotFoundException($"XML файл не НАЙДЕНН!!!   \"{result} \"");
            }

            return XElement.Load(result);
        }

        public static void SaveXmlFile(XDocument xDoc, string path)
        {
            xDoc.Save(Path.Combine(Directory.GetCurrentDirectory(), path));
        }
    }
}