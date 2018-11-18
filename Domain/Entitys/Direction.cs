using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Entitys
{
    public class Direction : EntityBase
    {
        public string Name { get; set; }
        public List<Station> Stations { get; set; }

        public Direction(int id, string name, List<Station> stations = null)
        {
            Id = id;
            Name = name;
            Stations = stations;
        }
        

        public Station GetStationInDirectionByName(string ruStationName)
        {
            return Stations?.FirstOrDefault(st => st.NameRu == ruStationName);
        }

        public Station GetStationInDirectionByNameIgnoreCase(string ruStationName)
        {
            return Stations?.FirstOrDefault(st => ruStationName?.IndexOf(st.NameRu, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public Station GetStationInDirectionByCode(int codeEsr, int codeExpress = 0)
        {
            return Stations?.FirstOrDefault(st => (codeEsr != 0 && st.CodeEsr == codeEsr) || (codeExpress != 0 && st.CodeExpress == codeExpress));
        }

        public IEnumerable<Station> GetStationsInDirectionByCode(int codeEsr, int codeExpress = 0)
        {
            return Stations?.Where(st => (codeEsr != 0 && st.CodeEsr == codeEsr) || (codeExpress != 0 && st.CodeExpress == codeExpress));
        }

        public override string ToString()
        {
            return Name;
        }
    }
}