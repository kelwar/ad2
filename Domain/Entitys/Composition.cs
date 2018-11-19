using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Domain.Entitys.Authentication;
using System.ComponentModel;
using Domain.Entitys.Train;

namespace Domain.Entitys
{
    public enum CarNumbering
    {
        [Description("Неизвестна")]
        Undefined,
        [Description("С головы")]
        Head,
        [Description("С хвоста")]
        Rear
    }
    public static class CarNumberingExtensions
    {
        public static string ToStringCarNumbering(this Enum enumerate)
        {
            var type = enumerate.GetType();
            var fieldInfo = type.GetField(enumerate.ToString());
            var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return (attributes.Length > 0) ? attributes[0].Description : enumerate.ToString();
        }
    }

    public class Composition : EntityBase
    {
        private bool isChange;

        public int VagonCount { get; set; }        // Количество вагонов
        public int LocomotiveCount { get; set; }   // Количество локомотивов
        public int Length { get; set; }            // Условная длина поезда
        public List<Vagon> Vagons { get; set; }
        public CarNumbering CarNumbering { get; set; }
        //{
        //    get
        //    {
        //        return (Vagons != null && Vagons.Count > 2) ? CalcCarNumbering() : CarNumbering.Undefined;
        //    }
        //    set
        //    {
        //        carNumbering = value;
        //    }
        //}

        public Composition()
        {
            VagonCount = 0;
            LocomotiveCount = 0;
            Length = 0;
            Vagons = new List<Vagon>();
            CarNumbering = CarNumbering.Undefined;
        }

        public Composition(int vagonCount = 0, int locomotiveCount = 0, int length = 0, List<Vagon> vagons = null)
        {
            VagonCount = vagonCount;
            LocomotiveCount = locomotiveCount;
            Vagons = vagons;

            var realLength = 0;
            if (Vagons != null)
            {
                foreach (var v in Vagons)
                {
                    realLength += v.Length;
                }
                if (length != realLength && realLength != 0)
                {
                    Library.Logs.Log.log.Warn($"Параметр UslDlPoezd не совпадает с реальной суммой длин вагонов состава. UslDlPoezd = {length}, RealLength = {realLength}");
                }
            }
            Length = realLength > 0 ? realLength : length;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var cmp = obj as Composition;
            if (cmp == null)
                return false;

            var isEquals = Vagons != null && cmp.Vagons != null ? Vagons.Count == cmp.Vagons.Count : false;
            if (isEquals)
            {
                for (var i = 0; isEquals && i < Vagons.Count; i++)
                {
                    isEquals = Vagons[i].Equals(cmp.Vagons[i]);
                }
            }
            
            isChange = false;
            return (isEquals || (Vagons == null && cmp.Vagons == null)) &&
                   VagonCount == cmp.VagonCount && 
                   LocomotiveCount == cmp.LocomotiveCount &&
                   Length == cmp.Length &&
                   CarNumbering == cmp.CarNumbering;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public void AddVagon(Vagon vagon, int position = -1)
        {
            if (Vagons == null)
                Vagons = new List<Vagon>();

            Insert(vagon, position);
            Length += vagon.Length;
            UpdateVagonsId();
        }

        public void Shuffle(int startPos, int endPos)
        {
            if (Vagons == null || startPos < 0 || Vagons.Count < startPos || endPos < 0 || Vagons.Count < endPos)
                return;

            var vagon = Vagons[startPos];
            Vagons.Remove(vagon);
            Insert(vagon, endPos);
            UpdateVagonsId();
        }

        public void Shuffle(Vagon vagon, int position = -1)
        {
            if (Vagons == null)
                return;

            Vagons.Remove(vagon);
            Insert(vagon, position);
            UpdateVagonsId();
        }

        public void EditVagon(Vagon vagon)
        {
            var index = Vagons?.IndexOf(vagon) ?? -1;
            isChange = Vagons != null && index >= 0 && index < Vagons.Count && !Vagons[index].Equals(vagon);
            if (isChange)
            {
                Library.Logs.Log.log.Info($"Изменен {index + 1}-й по счету вагон. Обновленные данные: " +
                                                        $"Длина вагона {vagon.Length}, номер {vagon.VagonNumber}, порядок расположения {vagon.VagonId}");
            }
        }

        public void RemoveVagon(int index)
        {
            if (Vagons == null || index < 0 || Vagons.Count < index)
                return;

            Length -= Vagons[index].Length;
            Library.Logs.Log.log.Info($"Удален {index + 1}-й вагон. Данные порядковых номеров обновлены");
            Vagons.RemoveAt(index);
            UpdateVagonsId();
            isChange = true;
        }

        public void UpdateVagonsId()
        {
            if (Vagons != null)
                foreach (var vagon in Vagons)
                {
                    vagon.VagonId = Vagons.IndexOf(vagon);
                }
        }

        public void Reverse()
        {
            if (Vagons != null && Vagons.Any())
            {
                Vagons.Reverse();
            }
        }

        public void ResortByNppVag()
        {
            if (Vagons != null && Vagons.Any())
            {
                var dict = new SortedDictionary<int, Vagon>();
                foreach (var v in Vagons)
                {
                    dict.Add(v.VagonId, v);
                }

                Vagons = new List<Vagon>(dict.Values);
            }
        }

        private void Insert(Vagon vagon, int position = -1)
        {
            if (vagon == null)
                return;

            if (position < 0 || position > Vagons.Count)
            {
                Library.Logs.Log.log.Info($"В конец поезда добавлен вагон №{vagon.VagonNumber}, длина {vagon.Length}");
                Vagons.Add(vagon);
            }
            else
            {
                Library.Logs.Log.log.Info($"{position + 1}-м вагоном добавлен вагон №{vagon.VagonNumber}, длина {vagon.Length}");
                Vagons.Insert(position, vagon);
            }
            isChange = true;
        }

        private CarNumbering CalcCarNumbering()
        {
            var cars = Vagons?.Where(v => v.PsType == PsType.Carriage && (byte)v.VagonType < 40)?.ToList() ?? null;
            if (cars == null || !cars.Any())
                return CarNumbering.Undefined;

            var result = cars[0].VagonNumber < cars.Sum(i => i.VagonNumber) / cars.Count ? CarNumbering.Head : CarNumbering.Rear;

            int trueCount = 0, falseCount = 0;
            for (var i = 1; i < cars.Count; i++)
            {
                if (cars[i].VagonNumber < cars[i - 1].VagonNumber)
                {
                    if (result == CarNumbering.Head)
                        falseCount++;
                    else if (result == CarNumbering.Rear)
                        trueCount++;
                }
                else if (cars[i].VagonNumber > cars[i - 1].VagonNumber)
                {
                    if (result == CarNumbering.Head)
                        trueCount++;
                    else if (result == CarNumbering.Rear)
                        falseCount++;
                }
            }

            if (trueCount < falseCount)
            {
                result = result == CarNumbering.Head ? CarNumbering.Rear : CarNumbering.Head;
            }

            return result;
        }

        private CarNumbering GetCarNumbering()
        {
            return Vagons != null && Vagons.Count > 2 ?
                        //Vagons.Count - Vagons.FirstOrDefault().VagonId < 2 ?
                        Vagons.FirstOrDefault().VagonId == 0 ?
                        CarNumbering.Head :
                        CarNumbering.Rear :
                   CarNumbering.Undefined;
        }

        public override string ToString()
        {
            var result = string.Empty;
            if (Vagons != null)
            {
                result += $"Вагоны:";
                Vagons.ForEach(v => result += $" {v}");
                result += $". ";
            }
            else
            {
                result += $"Информация о вагонах отсутствует. ";
            }
            result += $"Нумерация: {CarNumbering.ToStringCarNumbering()}";

            return result;
        }
    }
}
