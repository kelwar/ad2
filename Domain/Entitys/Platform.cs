using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entitys
{
    public class Platform : EntityBase
    {
        public string Name { get; set; }            // Название платформы
        public int Length { get; set; }             // Длина платформы
        public Station WhereFrom { get; set; }      // Откуда начинается вектор расположения секторов (с какой станции, перед первым элементом)
        public Station WhereTo { get; set; }        // В сторону какой станции идёт вектор расположения секторов
        public List<Sector> Sectors { get; set; }   // Список секторов платформы
    }
}
