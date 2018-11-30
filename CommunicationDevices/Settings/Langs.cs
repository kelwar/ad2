using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace CommunicationDevices.Settings
{
    public class Lang
    {
        public string Name { get; set; }
        public int Period { get; }
        public Lang Previous { get; set; }
        public Lang Next { get; set; }

        public Lang(string name = "Rus", int period = 10, Lang previous = null, Lang next = null)
        {
            Name = name;
            Period = period * 1000;
            Previous = previous;
            Next = next;
        }

        public Lang TurnLang()
        {
            return Next;
        }

        public override bool Equals(object obj)
        {
            var lng = obj as Lang;
            return lng != null && Name == lng.Name;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class Langs
    {
        public List<Lang> List { get; set; }
    }
}
