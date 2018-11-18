using Domain.Entitys;
using Library.Logs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CommunicationDevices.DataProviders.XmlDataProvider.XMLFormatProviders
{
    public class XmlStationsFormatProvider : IFormatProvider
    {
        public string CreateDoc(IEnumerable<UniversalInputType> tables)
        {
            var xDoc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), new XElement("Root"));

            try
            {
                if (tables == null || tables.Count() != 1)
                    return null;

                var uit = tables.FirstOrDefault();

                if (uit.ViewBag != null && uit.ViewBag.ContainsKey("Directions"))
                {
                    foreach (Direction dir in uit.ViewBag["Directions"])
                    {
                        var xDir = new XElement("Direction",
                                            new XAttribute("Id", dir?.Id ?? 0),
                                            new XAttribute("Name", dir?.Name ?? string.Empty));
                        foreach (var station in dir.Stations)
                        {
                            xDir.Add(new XElement("Station",
                                                    new XAttribute("Id", dir.Stations.IndexOf(station) + 1),
                                                    new XAttribute("NameRu", station?.NameRu ?? string.Empty),
                                                    new XAttribute("NameEng", station?.NameEng ?? string.Empty),
                                                    new XAttribute("NameCh", station?.NameCh ?? string.Empty),
                                                    new XAttribute("CodeEsr", station?.CodeEsr != 0 ? station?.CodeEsr.ToString() : string.Empty),
                                                    new XAttribute("CodeExpress", station?.CodeExpress != 0 ? station?.CodeExpress.ToString() : string.Empty),
                                                    new XAttribute("NearestStation", station?.NearestStation ?? string.Empty)));
                        }
                        xDoc.Root?.Add(xDir);
                    }
                }
            }
            catch (Exception ex)
            {
                Library.Logs.Log.log.Error(ex);
            }

            return xDoc.ToString();
        }
    }
}
