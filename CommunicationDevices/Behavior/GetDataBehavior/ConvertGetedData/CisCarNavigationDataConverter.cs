using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CommunicationDevices.DataProviders;
using Domain.Entitys;
using System.Text.RegularExpressions;
using System.Globalization;
using Library;

namespace CommunicationDevices.Behavior.GetDataBehavior.ConvertGetedData
{
    class CisCarNavigationDataConverter : IInputDataConverter
    {
        public IEnumerable<UniversalInputType> ParseXml2Uit(XDocument xDoc)
        {
            //Log.log.Trace("xDoc" + xDoc.ToString());//LOG;
            var shedules = new List<UniversalInputType>();
            
            List<XElement> lines = null;
            var inDataType = InDataType.Vagons;
            lines = xDoc.Element("trains")?.Elements("train")?.ToList();

            if (lines != null)
            {
                for (var i = 0; i < lines.Count; i++)
                //foreach (var line in lines)
                {
                    var line = lines[i];
                    var uit = new UniversalInputType
                    {
                        ViewBag = new Dictionary<string, dynamic>(),
                        TransitTime = new Dictionary<string, DateTime>()
                    };
                    uit.InDataType = inDataType;

                    try
                    {
                        var parser = Parser.GetParser();
                        uit.IsActive = true;
                        // Повагонная навигация
                        uit.ViewBag["StartDate"] = parser.ToDateTime(StringTrim(line, "DateFrom"));

                        uit.NumberOfTrain = StringTrim(line, "NomPoezd");
                        
                        uit.StationDeparture = new Station
                        {
                            CodeEsr = parser.ToInt(StringTrim(line, "KsnmPoezd"))
                        };
                        
                        uit.StationArrival = new Station
                        {
                            CodeEsr = parser.ToInt(StringTrim(line, "KskmPoezd"))
                        };
                        
                        uit.TrnId = parser.ToInt(StringTrim(line, "IdPoezd"));

                        DateTime dtPrib;
                        if (DateTime.TryParseExact(StringTrim(line, "Prib"), "yyyy-MM-dd HH:mm:ss.f", CultureInfo.InvariantCulture, DateTimeStyles.None, out dtPrib))
                        {
                            uit.TransitTime["приб"] = TimeZoneInfo.ConvertTime(dtPrib, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"), TimeZoneInfo.Local);
                        }

                        DateTime dtOtpr;
                        if (DateTime.TryParseExact(StringTrim(line, "Otpr"), "yyyy-MM-dd HH:mm:ss.f", CultureInfo.InvariantCulture, DateTimeStyles.None, out dtOtpr))
                        {
                            uit.TransitTime["отпр"] = TimeZoneInfo.ConvertTime(dtOtpr, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"), TimeZoneInfo.Local);
                        }

                        uit.VagonDirection = (VagonDirection)parser.ToByte(StringTrim(line, "CarNumbering"));

                        int number;
                        PsType psType;
                        VagonType vagonType;
                        var underLines = line?.Element("vagons")?.Elements("v")?.ToList();
                        List<Vagon> vagons = null;
                        if (underLines != null && underLines.Count > 1)// && underLines.FirstOrDefault().Name != "v")
                        {
                            vagons = new List<Vagon>();
                            foreach (var underLine in underLines)
                            {
                                try
                                {
                                    vagons.Add(new Vagon
                                    (
                                        int.TryParse(StringTrim(underLine, "NppVag"), out number) ? number : 0,
                                        StringTrim(underLine, "NomPS"),
                                        int.TryParse(StringTrim(underLine, "NomVagShem"), out number) ? number : 0,
                                        Enum.TryParse(StringTrim(underLine, "TipPS"), out psType) ? psType : PsType.Other,
                                        Enum.TryParse(StringTrim(underLine, "TipVag"), out vagonType) ? vagonType : VagonType.Other,
                                        int.TryParse(StringTrim(underLine, "Dlina"), out number) ? number : 0,
                                        int.TryParse(StringTrim(underLine, "NomVagShemAsupv"), out number) ? number : 0,
                                        int.TryParse(StringTrim(underLine, "NomVagShemAsoup"), out number) ? number : 0
                                    ));
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Ошибка: {ex}");
                                }
                            }
                        }

                        uit.ViewBag["Composition"] = new Composition(
                            int.TryParse(StringTrim(line, "KolVag"), out number) ? number : 0,
                            int.TryParse(StringTrim(line, "KolLok"), out number) ? number : 0,
                            int.TryParse(StringTrim(line, "UslDlPoezd"), out number) ? number : 0,
                            vagons);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка: {ex}");
                    }

                    shedules.Add(uit);
                }
            }


            return shedules;
        }
        private string StringTrim(XElement line, string s)
        {
            var elem = line?.Element(s)?.Value.Replace("\\", "/") ?? string.Empty;
            return Regex.Replace(elem.TrimEnd(), "[\r\n\t]+", "");
        }
    }
}
