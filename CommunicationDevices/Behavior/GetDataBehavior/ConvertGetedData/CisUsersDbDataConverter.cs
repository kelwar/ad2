using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CommunicationDevices.DataProviders;
using System.Text.RegularExpressions;

namespace CommunicationDevices.Behavior.GetDataBehavior.ConvertGetedData
{
    class CisUsersDbDataConverter : IInputDataConverter
    {
        public IEnumerable<UniversalInputType> ParseXml2Uit(XDocument xDoc)
        {
            //Log.log.Trace("xDoc" + xDoc.ToString());//LOG;
            var shedules = new List<UniversalInputType>();

            List<XElement> lines = null;

            var inDataType = InDataType.Users;
            lines = xDoc.Element("users")?.Elements("user")?.ToList();

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
                        //login------------
                        uit.ViewBag["login"] = StringTrim(line, "login");

                        //password------------
                        uit.ViewBag["hash_salt_pass"] = StringTrim(line, "hash_salt_pass");

                        //role id------------
                        int role_id;
                        uit.ViewBag["role"] = int.TryParse(StringTrim(line, "role"), out role_id) ? role_id : 0;

                        //ФИО ------------
                        uit.ViewBag["FullName"] = $"{StringTrim(line, "surname")} {StringTrim(line, "name")?.FirstOrDefault()}.{StringTrim(line, "patronymic")?.FirstOrDefault()}.";

                        //Status------------
                        int status_id;
                        uit.ViewBag["status"] = int.TryParse(StringTrim(line, "status"), out status_id) ? status_id == 2 : false;

                        //Start_date------------
                        DateTime date;
                        uit.ViewBag["start_date"] = DateTime.TryParse(StringTrim(line, "start_date"), out date) ? date : new DateTime(1900, 01, 01);
                        //ent_date------------
                        uit.ViewBag["end_date"] = DateTime.TryParse(StringTrim(line, "end_date"), out date) ? date : new DateTime(2100, 12, 31);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка: {ex.Message}");
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
