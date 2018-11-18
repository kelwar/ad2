using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationDevices.Settings.XmlDeviceSettings.XmlSpecialSettings
{
    public class XmlLangSetting
    {
        public Langs Langs { get; }

        public XmlLangSetting(string langs)
        {
            var _langs = langs?.Split(';') ?? null;
            if (_langs != null && _langs.Any())
            {
                Langs = new Langs()
                {
                    List = new List<Lang>()
                };
                
                foreach (var lang in _langs)
                {
                    var l = ParseLangString(lang);
                    Langs.List.Add(l);
                }

                var count = Langs.List.Count;
                for (var i = 0; i < count; i++)
                {
                    var list = Langs.List;
                    var lang = list[i];
                    lang.Previous = list[i > 0 ? i - 1 : count - 1];
                    lang.Next = list[i < count - 1 ? i + 1 : 0];
                }
            }
        }

        private Lang ParseLangString(string lang)
        {
            var l = lang.Split(':');
            var name = l[0];
            int period;
            return (l.Length > 1 && int.TryParse(l[1], out period)) ? new Lang(name, period) : new Lang(name);
        }
    }
}
