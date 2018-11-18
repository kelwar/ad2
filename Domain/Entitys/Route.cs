using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entitys
{
    public class Route
    {
        public IDictionary<int, Stop> Stops { get; }

        public Route()
        {
            Stops = new SortedDictionary<int, Stop>();
        }

        public string GetString(NotificationLanguage lang = NotificationLanguage.Ru)
        {
            if (!Stops.Any())
                return string.Empty;

            int stopCount = 0;
            int nonStopCount = 0;
            foreach (var s in Stops)
            {
                if (s.Value == null)
                    continue;

                if (s.Value.Station != null && s.Value.StopState == StopState.TechNonStop)
                    s.Value.StopState = StopState.NonStop;

                switch (s.Value.StopState)
                {
                    case StopState.Stop:
                        stopCount++;
                        break;
                    case StopState.NonStop:
                        nonStopCount++;
                        break;
                    case StopState.TechNonStop:
                        break;
                }
            }

            if (nonStopCount == 0) return GetWithAllStopsString(lang);
            else if (stopCount < nonStopCount) return GetWithStopString(lang);
            else return GetWithoutStopString(lang);
        }

        private string GetWithAllStopsString(NotificationLanguage lang = NotificationLanguage.Ru)
        {
            var result = string.Empty;
            switch (lang)
            {
                case NotificationLanguage.Ru:
                    result = "Со всеми остановками";
                    break;
                case NotificationLanguage.Eng:
                    result = "With all stops";
                    break;                
            }
            return result;
        }

        private string GetWithStopString(NotificationLanguage lang = NotificationLanguage.Ru)
        {
            var result = string.Empty;
            switch (lang)
            {
                case NotificationLanguage.Ru:
                    result = "С остановками: ";
                    break;
                case NotificationLanguage.Eng:
                    result = "With stops: ";
                    break;
            }

            var isFirstStop = true;
            foreach (var s in Stops)
            {
                if (s.Value == null || s.Value.Station == null)
                    continue;

                var stationName = string.Empty;
                switch (lang)
                {
                    case NotificationLanguage.Ru:
                        stationName = s.Value.Station?.NameRu ?? string.Empty;
                        break;
                    case NotificationLanguage.Eng:
                        stationName = s.Value.Station?.NameEng ?? string.Empty;
                        break;
                }

                if (s.Value.StopState == StopState.Stop)
                {
                    if (isFirstStop)
                        isFirstStop = false;
                    else
                        result += ",";

                    result += stationName;
                }
            }
            return result;
        }

        private string GetWithoutStopString(NotificationLanguage lang = NotificationLanguage.Ru)
        {
            var result = string.Empty;
            switch (lang)
            {
                case NotificationLanguage.Ru:
                    result = "Кроме: ";
                    break;
                case NotificationLanguage.Eng:
                    result = "Except: ";
                    break;
            }

            var isFirstNonStop = true;
            foreach (var s in Stops)
            {
                if (s.Value == null || s.Value.Station == null)
                    continue;

                var stationName = string.Empty;
                switch (lang)
                {
                    case NotificationLanguage.Ru:
                        stationName = s.Value.Station?.NameRu ?? string.Empty;
                        break;
                    case NotificationLanguage.Eng:
                        stationName = s.Value.Station?.NameEng ?? string.Empty;
                        break;
                }

                if (s.Value.StopState == StopState.NonStop)
                {
                    if (isFirstNonStop)
                        isFirstNonStop = false;
                    else
                        result += ",";

                    result += stationName;
                }
            }
            return result;
        }
    }
}
