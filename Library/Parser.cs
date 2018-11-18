using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Library
{
    public class Parser
    {
        private static Parser _instance;

        private Parser() { }

        public static Parser GetParser()
        {
            if (_instance == null)
                _instance = new Parser();
            return _instance;
        }

        public int ToInt(string data)
        {
            int result;
            return int.TryParse(data, out result) ? result : 0;
        }

        public byte ToByte(string data)
        {
            byte result;
            return byte.TryParse(data, out result) ? result : (byte)0;
        }

        public DateTime ToDateTime(string data, string format = null)
        {
            DateTime result;
            return string.IsNullOrWhiteSpace(format) && DateTime.TryParse(data, out result) || 
                   DateTime.TryParseExact(data, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out result) ? result : DateTime.MinValue;
        }

        public TimeSpan ToTimeSpan(string data)
        {
            TimeSpan result;
            return TimeSpan.TryParse(data, out result) ? result : TimeSpan.MinValue;
        }

        public bool ToBool(string data)
        {
            bool result;
            return bool.TryParse(data, out result) ? result : ToInt(data) > 0;
        }
    }
}
