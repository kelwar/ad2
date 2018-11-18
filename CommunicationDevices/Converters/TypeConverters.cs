﻿using CommunicationDevices.DataProviders;
using NLog.LayoutRenderers;

namespace CommunicationDevices.Converters
{
    internal static class TypeConverters
    {
        public enum TypeTrainViewFormat { Long, Short }

        public static string TypeTrainEnum2RusString(TypeTrain typeTrain, TypeTrainViewFormat trainViewFormat)
        {
            switch (typeTrain)
            {
                case TypeTrain.None:
                    return " ";

                case TypeTrain.Passenger:
                    return (trainViewFormat == TypeTrainViewFormat.Long) ? "Пассажирский" : "пасс";

                case TypeTrain.Suburban:
                    return (trainViewFormat == TypeTrainViewFormat.Long) ? "Пригородный" : "приг";

                case TypeTrain.Corporate:
                    return (trainViewFormat == TypeTrainViewFormat.Long) ? "Фирменный" : "фирм";

                case TypeTrain.Express:
                    return (trainViewFormat == TypeTrainViewFormat.Long) ? "Скорый" : "скор";

                case TypeTrain.HighSpeed:
                    return (trainViewFormat == TypeTrainViewFormat.Long) ? "Скоростной" : "скорост";

                case TypeTrain.Swallow:
                    return (trainViewFormat == TypeTrainViewFormat.Long) ? "Экспресс" : "эксп";

                case TypeTrain.Rex:
                    return (trainViewFormat == TypeTrainViewFormat.Long) ? "Экспресс" : "эксп";
            }

            return string.Empty;
        }

        public static string TypeTrainEnum2EngString(TypeTrain typeTrain, TypeTrainViewFormat trainViewFormat)
        {
            switch (typeTrain)
            {
                case TypeTrain.None:
                    return " ";

                case TypeTrain.Passenger:
                    return (trainViewFormat == TypeTrainViewFormat.Long) ? "Passenger" : "pass";

                case TypeTrain.Suburban:
                    return (trainViewFormat == TypeTrainViewFormat.Long) ? "Suburban" : "sub";

                case TypeTrain.Corporate:
                    return (trainViewFormat == TypeTrainViewFormat.Long) ? "Corporate" : "corp";

                case TypeTrain.Express:
                    return (trainViewFormat == TypeTrainViewFormat.Long) ? "Express" : "expr";

                case TypeTrain.HighSpeed:
                    return (trainViewFormat == TypeTrainViewFormat.Long) ? "High-Speed" : "h-sp";

                case TypeTrain.Swallow:
                    return (trainViewFormat == TypeTrainViewFormat.Long) ? "Express" : "expr";

                case TypeTrain.Rex:
                    return (trainViewFormat == TypeTrainViewFormat.Long) ? "Express" : "expr";
            }

            return string.Empty;
        }
    }
}