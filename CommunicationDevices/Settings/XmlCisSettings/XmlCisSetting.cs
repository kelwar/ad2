using System.Xml.Linq;

namespace CommunicationDevices.Settings.XmlCisSettings
{

    public class XmlCisSetting
    {
        #region prop

        public string Name { get; }
        public string NameEng { get; }
        public string NameCh { get; }
        public string EndpointAddress { get; }
        public int? CodeEsr { get; }
        public int? CodeExpress { get; }

        #endregion




        #region ctor

        private XmlCisSetting(string name, string nameEng, string nameCh, string endpointAddress, int? codeEsr = 0, int? codeExpress = 0)
        {
            Name = name;
            NameEng = nameEng;
            NameCh = nameCh;
            EndpointAddress = endpointAddress;
            CodeEsr = codeEsr;
            CodeExpress = codeExpress;
        }

        #endregion




        #region Methode

        /// <summary>
        /// Обязательно вызывать в блоке try{}
        /// </summary>
        public static XmlCisSetting LoadXmlSetting(XElement xml)
        {
            return new XmlCisSetting(
                           (string)xml?.Element("CisSetting")?.Element("Name"),
                           (string)xml?.Element("CisSetting")?.Element("NameEng"),
                           (string)xml?.Element("CisSetting")?.Element("NameCh"),
                           (string)xml?.Element("CisSetting")?.Element("EndpointAddress"),
                           (int?)xml?.Element("CisSetting")?.Element("CodeEsr") ?? 0,
                           (int?)xml?.Element("CisSetting")?.Element("CodeExpress") ?? 0
                          );
        }

        #endregion
    }
}