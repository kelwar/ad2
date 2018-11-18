using Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entitys.Train
{
    public class TrainNumber
    {
        public int Num1 { get; set; }
        public int Num2 { get; set; }

        public TrainNumber(int num1, int num2)
        {
            Num1 = num1;
            Num2 = num2;
        }

        public TrainNumber(string num1, string num2)
        {
            var parser = Parser.GetParser();
            Num1 = parser.ToInt(num1);
            Num2 = parser.ToInt(num2);
        }

        public TrainNumber(string num)
        {
            var s = num.Split('/');
            var parser = Parser.GetParser();
            Num1 = s.Length > 0 ? parser.ToInt(s[0]) : 0;
            Num2 = s.Length > 1 ? parser.ToInt(s[1]) : 0;
        }
    }
}
