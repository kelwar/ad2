using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entitys
{
    public enum PsType
    {
        Other = 4, Locomotive = 1, Carriage = 2, MotorCar = 3
    }

    public enum VagonType
    {
        Other = 90, Luxury = 01, Coupe = 10, PlatzCard = 20, Common = 30, Post = 40, Baggage = 50
    }
     
    public class Vagon : EntityBase
    {
        public int VagonId { get; set; }               // Порядковый номер вагона в составе поезда
        public int VagonNumber { get; set; }        // Номер вагона
        public PsType PsType { get; set; }             // Тип подвижного состава
        public VagonType VagonType { get; set; }       // Тип вагона
        public int Length { get; set; }                // Длина подвижного состава
        public string UniqueVagonId { get; }
        public int VagonNumberAsupv { get; set; }        // Номер вагона
        public int VagonNumberAsoup { get; set; }        // Номер вагона

        public Vagon()
        {
            VagonId = -1;
            UniqueVagonId = string.Empty;
            VagonNumber = -1;
            PsType = PsType.Other;
            VagonType = VagonType.Other;
            Length = 0;
            VagonNumberAsupv = -1;
            VagonNumberAsoup = -1;
        }

        public Vagon(int vagonId, string uniqueVagonId = "", int vagonNumber = -1, PsType psType = PsType.Other, VagonType vagonType = VagonType.Other, int length = 0, int vagonNumberAsupv = -1, int vagonNumberAsoup = -1)
        {
            VagonId = vagonId;
            UniqueVagonId = uniqueVagonId;
            VagonNumber = vagonNumber;
            PsType = psType;
            VagonType = vagonType;
            Length = length;
            VagonNumberAsupv = vagonNumberAsupv;
            VagonNumberAsoup = vagonNumberAsoup;
        }

        public Vagon(Vagon vagon)
        {
            UniqueVagonId = vagon?.UniqueVagonId ?? string.Empty;
            VagonId = vagon?.VagonId ?? -1;
            VagonNumber = vagon?.VagonNumber ?? -1;
            PsType = vagon?.PsType ?? PsType.Other;
            VagonType = vagon?.VagonType ?? VagonType.Other;
            Length = vagon?.Length ?? 0;
            VagonNumberAsupv = vagon?.VagonNumberAsupv ?? -1;
            VagonNumberAsoup = vagon?.VagonNumberAsoup ?? -1;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var vag = obj as Vagon;
            if (vag == null)
                return false;

            return UniqueVagonId == vag.UniqueVagonId &&
                   VagonId == vag.VagonId &&
                   VagonNumber == vag.VagonNumber &&
                   PsType == vag.PsType &&
                   VagonType == vag.VagonType &&
                   Length == vag.Length &&
                   VagonNumberAsupv == vag.VagonNumberAsupv &&
                   VagonNumberAsoup == vag.VagonNumberAsoup;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return VagonNumber.ToString();
        }
    }
}
