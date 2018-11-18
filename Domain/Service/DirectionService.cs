using Domain.Abstract;
using Domain.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Service
{
    public class DirectionService
    {
        private IRepository<Direction> _directionRepository;


        public DirectionService(IRepository<Direction> directionRepository)
        {
            _directionRepository = directionRepository;
        }
        
        public string GetDirection(Station startStation, Station endStation, int trainNumber = 0)
        {
            return GetMainDir(GetIntersectingDirections(GetDirectionsByStation(startStation), GetDirectionsByStation(endStation)), startStation, endStation, trainNumber)?.Name ?? string.Empty;
        }

        public IEnumerable<Station> GetStationsByCode(int codeEsr, int codeExpress = 0)
        {
            List<Station> _stations = null;
            foreach (var direction in _directionRepository.List())
            {
                var stations = direction.GetStationsInDirectionByCode(codeEsr, codeExpress);
                if (stations != null)
                {
                    if (_stations == null)
                        _stations = new List<Station>();
                    _stations.AddRange(stations);
                }
            }
            return _stations;
        }

        public Station GetStationByCode(int codeEsr, int codeExpress, string name)
        {
            Station _station = null;
            if (codeEsr == 0 && codeExpress == 0 && string.IsNullOrWhiteSpace(name))
            {
                return null;
            }
            foreach (var direction in _directionRepository.List())
            {
                var station = direction.GetStationInDirectionByCode(codeEsr, codeExpress);
                if (station != null)
                {
                    _station = station;
                }
            }
            if (_station == null)
            {
                _station = GetStationByName(name);
            }
            return _station;
        }
        
        public Route GetRouteWithNormalStopNames(Route route)
        {
            if (route == null)
                return null;

            foreach (var s in route.Stops)
            {
                if (s.Value == null || s.Value.Station == null)
                    continue;

                route.Stops[s.Key].Station = GetStationByCode(s.Value.Station.CodeEsr);
            }

            return route;
        }


        private IEnumerable<Direction> GetIntersectingDirections(IEnumerable<Direction> startDirs, IEnumerable<Direction> endDirs)
        {
            if (startDirs == null || endDirs == null)
                return null;

            return startDirs.Where(sd => endDirs.ToList().Exists(ed => ed.Name == sd.Name));
        }

        private Direction GetMainDir(IEnumerable<Direction> intersectingDirections, Station startStation = null, Station endStation = null, int trainNumber = 0)
        {
            if (startStation != null && endStation != null)
            {
                intersectingDirections = intersectingDirections != null && intersectingDirections.Any() && 
                                         intersectingDirections.ToList().Exists(dir => dir.Stations.IndexOf(startStation) < dir.Stations.IndexOf(endStation)) ? 
                                         intersectingDirections.Where(dir => dir.Stations.IndexOf(startStation) < dir.Stations.IndexOf(endStation)) : 
                                         intersectingDirections;
            }

            if (intersectingDirections != null && 
                intersectingDirections.ToList().Exists(dir => dir.Name.ToLower().Contains("круглогодичн")) &&
                intersectingDirections.ToList().Exists(dir => dir.Name.ToLower().Contains("сезонн")) &&
                trainNumber > 0)
            {
                if (trainNumber < 150 || 
                    (trainNumber > 300 && trainNumber < 450) ||
                    (trainNumber > 700 && trainNumber < 800))
                {
                    intersectingDirections = intersectingDirections.Where(dir => dir.Name.ToLower().Contains("круглогодичн"));
                }
                else
                {
                    intersectingDirections = intersectingDirections.Where(dir => dir.Name.ToLower().Contains("сезонн"));
                }
            }

            return intersectingDirections?.FirstOrDefault(dir => dir.Stations.Count == intersectingDirections.Max(d => d.Stations.Count)) ?? null;
        }        

        private IEnumerable<Direction> GetDirectionsByStation(Station station)
        {
            return station != null ? (GetDirectionsByCode(station.CodeEsr, station.CodeExpress) ?? GetDirectionsByName(station.NameRu)) : null;
        }

        private IEnumerable<Direction> GetDirectionsByCode(int codeEsr, int codeExpress)
        {
            return _directionRepository.List().Where(d => d.GetStationInDirectionByCode(codeEsr, codeExpress) != null);
        }

        private IEnumerable<Direction> GetDirectionsByName(string ruStationName)
        {
            return _directionRepository.List().Where(d => d.GetStationInDirectionByNameIgnoreCase(ruStationName) != null);
        }

        private Station GetStationByCode(int codeEsr, int codeExpress = 0)
        {
            Station _station = null;
            foreach (var direction in _directionRepository.List())
            {
                var station = direction.GetStationInDirectionByCode(codeEsr, codeExpress);
                if (station != null)
                {
                    _station = station;
                    break;
                }
            }
            return _station;
        }

        private Station GetStationByName(string name)
        {
            Station _station = null;
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            foreach (var direction in _directionRepository.List())
            {
                var station = direction.GetStationInDirectionByNameIgnoreCase(name);
                if (station != null)
                {
                    _station = station;
                }
            }
            return _station;
        }
    }
}
