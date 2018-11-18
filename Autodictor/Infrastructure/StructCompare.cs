﻿using System.Xml;

namespace MainExample.Infrastructure
{
    public static class StructCompare
    {
        public static bool SoundRecordComparer (ref SoundRecord sr1, ref SoundRecord sr2)
        {
            return (sr1.ВремяОтправления == sr2.ВремяОтправления) &&
                   (sr1.ВремяПрибытия == sr2.ВремяПрибытия) &&
                   (sr1.ВремяСтоянки == sr2.ВремяСтоянки) &&
                   (sr1.ВремяЗадержки == sr2.ВремяЗадержки) &&
                   //(sr1.DelayTime == null && sr2.DelayTime == null || sr1.DelayTime == sr2.DelayTime) &&
                   //(sr1.ОжидаемоеВремя == sr2.ОжидаемоеВремя) &&
                   (sr1.ВремяСледования == sr2.ВремяСледования) &&
                   (sr1.ДниСледования == sr2.ДниСледования) &&
                   (sr1.СтанцияНазначения == sr2.СтанцияНазначения) &&
                   (sr1.СтанцияОтправления == sr2.СтанцияОтправления) &&
                   (sr1.НазваниеПоезда == sr2.НазваниеПоезда) &&
                   (sr1.НомерПоезда == sr2.НомерПоезда) &&
                   (sr1.НомерПоезда2 == sr2.НомерПоезда2) &&
                   (string.IsNullOrWhiteSpace(sr1.Примечание) && string.IsNullOrWhiteSpace(sr2.Примечание) || sr1.Примечание == sr2.Примечание) &&
                   (sr1.РазрешениеНаОтображениеПути == sr2.РазрешениеНаОтображениеПути) &&
                   (sr1.Активность == sr2.Активность) &&
                   (sr1.БитыНештатныхСитуаций == sr2.БитыНештатныхСитуаций) &&
                   (string.IsNullOrWhiteSpace(sr1.НомерПути) && string.IsNullOrWhiteSpace(sr2.НомерПути) || sr1.НомерПути == sr2.НомерПути) &&
                   (string.IsNullOrWhiteSpace(sr1.Дополнение) && string.IsNullOrWhiteSpace(sr2.Дополнение) || sr1.Дополнение == sr2.Дополнение) &&
                   (sr1.Автомат == sr2.Автомат) &&
                   (sr1.ИспользоватьДополнение == null && sr2.ИспользоватьДополнение == null || 
                   (sr1.ИспользоватьДополнение != null && sr1.ИспользоватьДополнение["табло"] == sr2.ИспользоватьДополнение["табло"])) &&
                   (sr1.НумерацияПоезда == sr2.НумерацияПоезда) &&
                   (sr1.Composition == null && sr2.Composition == null || (sr1.Composition != null && sr2.Composition != null && sr1.Composition.Equals(sr2.Composition)));
        }
    }
}