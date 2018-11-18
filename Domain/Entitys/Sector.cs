using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entitys
{
    public class Sector : EntityBase
    {
        public string Name { get; set; } // Название сектора "А", "B", "C" например
        public string Color { get; set; } // Цвет сектора
        public int Length { get; set; }  // Длина сектора
        public int Offset { get; set; }  // Смещение сектора относительно начала или предыдущего объекта (пустота)
    }
}
