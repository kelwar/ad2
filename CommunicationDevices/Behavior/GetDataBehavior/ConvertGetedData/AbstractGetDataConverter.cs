using CommunicationDevices.DataProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CommunicationDevices.Behavior.GetDataBehavior.ConvertGetedData
{
    public abstract class AbstractGetDataConverter
    {
        protected string StringTrim(XElement line, string s)
        {
            return Regex.Replace((line?.Element(s)?.Value.Replace("\\", "/") ?? string.Empty).TrimEnd(), "[\r\n\t]+", "");
        }
        protected string StringTrimToTitleCase(XElement line, string s)
        {
            return System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(StringTrim(line, s).ToLower());
        }

        //public abstract IEnumerable<UniversalInputType> ParseXml2Uit(XDocument xDoc);
    }
}
