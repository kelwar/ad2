using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;
using Domain.Abstract;
using Domain.Entitys;
using Library.Xml;

namespace Domain.Concrete
{
    public class RepositoryXmlDirection : IRepository<Direction>
    {
        private readonly XElement _xElement;
        private readonly string _filename;
        private IEnumerable<Direction> Directions { get; set; }



        public RepositoryXmlDirection(XElement xElement, string filename)
        {
            _xElement = xElement;
            _filename = filename;
        }



        public Direction GetById(int id)
        {
            throw new NotImplementedException();
        }

        public Direction GetByName(string directionName)
        {
            return List().FirstOrDefault(d => d.Name == directionName);
        }


        public IEnumerable<Direction> List()
        {
            return Directions ?? (Directions = ParseXmlFile());
        }
        
        public IEnumerable<Direction> Load()
        {
            return Directions = ParseXmlFile();
        }

        public void Save()
        {
            SaveToXmlFile();
        }

        private IEnumerable<Direction> ParseXmlFile()
        {
            var directions = new List<Direction>();
            try
            {
                foreach (var directXml in XmlWorker.LoadXmlFile(_filename).Elements("Direction"))
                {
                    var direct = new Direction(int.Parse((string)directXml.Attribute("Id")),
                                               (string)directXml.Attribute("Name"),
                                               new List<Station>());
                    try
                    {
                        var stations = directXml.Elements("Station").ToList();
                        if (stations.Any())
                        {
                            foreach (var stXml in stations)
                            {
                                try
                                {
                                    int number;
                                    direct.Stations.Add(new Station
                                    {
                                        Id = int.TryParse((string)stXml.Attribute("Id"), out number) ? number : 0,
                                        NameRu = (string)stXml.Attribute("NameRu"),
                                        NameEng = (string)stXml.Attribute("NameEng"),
                                        //NameCh = (stXml.Attribute("NameCh") != null) ? (string)stXml.Attribute("NameCh") : string.Empty,
                                        NameCh = (string)stXml.Attribute("NameCh"),
                                        CodeEsr = int.TryParse((string)stXml.Attribute("CodeEsr"), out number) ? number : 0,
                                        CodeExpress = int.TryParse((string)stXml.Attribute("CodeExpress"), out number) ? number : 0,
                                        NearestStation = (string)stXml.Attribute("NearestStation")
                                    });
                                }
                                catch (Exception ex)
                                {
                                    Library.Logs.Log.log.Error(ex);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Library.Logs.Log.log.Error(ex);
                    }
                    finally
                    {
                        directions.Add(direct);
                    }
                }
            }
            catch (Exception ex)
            {
                Library.Logs.Log.log.Error(ex);
            }
            return directions;
        }

        private void SaveToXmlFile()
        {
            var xDoc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), new XElement("Root"));

            try
            {
                foreach (var dir in Directions)
                {
                    var xDir = new XElement("Direction",
                                        new XAttribute("Id", dir?.Id ?? 0),
                                        new XAttribute("Name", dir?.Name ?? string.Empty));
                    foreach (var station in dir.Stations)
                    {
                        xDir.Add(new XElement("Station",
                                                new XAttribute("Id", dir.Stations.IndexOf(station) + 1),
                                                new XAttribute("NameRu", station?.NameRu ?? string.Empty),
                                                new XAttribute("NameEng", station?.NameEng ?? string.Empty),
                                                new XAttribute("NameCh", station?.NameCh ?? string.Empty),
                                                new XAttribute("CodeEsr", station?.CodeEsr != 0 ? station?.CodeEsr.ToString() : string.Empty),
                                                new XAttribute("CodeExpress", station?.CodeExpress != 0 ? station?.CodeExpress.ToString() : string.Empty),
                                                new XAttribute("NearestStation", station?.NearestStation ?? string.Empty)));
                    }
                    xDoc.Root?.Add(xDir);
                }

                XmlWorker.SaveXmlFile(xDoc, _filename);
            }
            catch (Exception ex)
            {
                Library.Logs.Log.log.Error(ex);
            }
        }

        public IEnumerable<Direction> List(Expression<Func<Direction, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public void Add(Direction entity)
        {
            Directions.ToList().Add(entity);
        }

        public void Delete(Direction entity)
        {
            Directions.ToList().Remove(entity);
        }

        public void Edit(Direction entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(Expression<Func<Direction, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public void AddRange(IEnumerable<Direction> entity)
        {
            throw new NotImplementedException();
        }


    }
}