using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entitys;
using MainExample.Entites;

namespace MainExample.Services
{
    public class SchedulingPipelineService
    {
        public bool CheckTrainActuality(ref TrainTableRecord config, DateTime dateCheck, Func<int, bool> limitationTime, byte workWithNumberOfDays)
        {
            var планРасписанияПоезда = TrainSchedule.ПолучитьИзСтрокиПланРасписанияПоезда(config.Days, config.ВремяНачалаДействияРасписания, config.ВремяОкончанияДействияРасписания);
            if ((workWithNumberOfDays == 7) || (планРасписанияПоезда.ScheduleMode != ScheduleMode.ПоДням) || (config.ТипПоезда == ТипПоезда.Пассажирский) || (config.ТипПоезда == ТипПоезда.Скоростной) || (config.ТипПоезда == ТипПоезда.Скорый))
            {
                var активностьНаДень = планРасписанияПоезда.ПолучитьАктивностьДняДвижения(dateCheck.Date, dateCheck.Date);
                if (активностьНаДень == false)
                    return false;

                if (limitationTime != null)
                {
                    DateTime времяПрибытия;
                    DateTime времяОтправления;

                    bool приб = DateTime.TryParse(config.ArrivalTime, out времяПрибытия);
                    bool отпр = DateTime.TryParse(config.DepartureTime, out времяОтправления);

                    if (приб && отпр) //ТРАНЗИТ
                    {
                        if (!limitationTime(времяОтправления.Hour))
                            return false;
                    }
                    else
                    if (приб)
                    {
                        if (!limitationTime(времяПрибытия.Hour))
                            return false;
                    }
                    else
                    if (отпр)
                    {
                        if (!limitationTime(времяОтправления.Hour))
                            return false;
                    }
                }
            }
            else
            {
                var day = ((int)dateCheck.DayOfWeek + 6) % 7;
                //var currentDay = dateCheck.Day;
                if (dateCheck > DateTime.Now)
                {
                    workWithNumberOfDays = (byte)((++workWithNumberOfDays) % 7);
                }

                var day2 = 0;
                if (dateCheck.Day <= 15)
                {
                    if (day > workWithNumberOfDays)
                        day2 += 7;
                }
                else
                {
                    if (day < workWithNumberOfDays)
                        day2 -= 7;
                }
                day2 += workWithNumberOfDays - day;

                //if (планРасписанияПоезда.ПолучитьАктивностьДняДвижения((byte)(dateCheck.Month - 1), (byte)(currentDay + day2 - 1), dateCheck) == false)
                if (планРасписанияПоезда.ПолучитьАктивностьДняДвижения(new DateTime(dateCheck.Year, dateCheck.Month, dateCheck.Day + day2).Date, dateCheck.Date) == false)
                    return false;
            }

            return true;
        }


        public string GetUniqueKey(IEnumerable<string> currentKeys,  DateTime addingKey)
        {
            int tryCounter = 50;
            while (--tryCounter > 0)
            {
                string key = addingKey.ToString(MainWindowForm.DATETIME_KEYFORMAT);

                if (!currentKeys.Contains(key))
                {
                    return key;
                }
                addingKey = addingKey.AddSeconds(1);
            }

            return null;

           // throw new Exception($"Невозможно добавить запись под ключем: {addingKey:yy.MM.dd  HH:mm:ss}");
        }

    }
}