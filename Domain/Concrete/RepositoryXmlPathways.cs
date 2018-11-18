using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Xml.Linq;
using Domain.Abstract;
using Domain.Entitys;

namespace Domain.Concrete
{
    public class RepositoryXmlPathways : IRepository<Pathways>
    {
        private readonly XElement _xElement;
        private IEnumerable<Pathways> Pathways { get; set; }



        public RepositoryXmlPathways(XElement xElement)
        {
            _xElement = xElement;
        }




        public Pathways GetById(int id)
        {
            throw new NotImplementedException();
        }

        public Pathways GetByName(string name)
        {
            throw new NotImplementedException();
        }



        public IEnumerable<Pathways> List()
        {
          return Pathways ?? (Pathways = ParseXmlFile());     
        }



        private IEnumerable<Pathways> ParseXmlFile()
        {
            var pathWays = new List<Pathways>();
            try
            {
                foreach (var directXml in _xElement.Elements("Path"))
                {
                    Platform platform = null;

                    if (directXml.Element("Platform") != null)
                    {
                        var plat = directXml.Element("Platform");
                        var whereFrom = (string)plat.Attribute("WhereFrom");
                        var whereTo = (string)plat.Attribute("WhereTo");
                        var sectors = new List<Sector>();
                        foreach (var sector in plat.Elements("Sector"))
                        {
                            int sectorLength, offset;
                            sectors.Add(new Sector
                            {
                                Name = (string)sector.Attribute("Name"),
                                Color = (string)sector.Attribute("Color"),
                                Length = int.TryParse((string)sector.Attribute("Length"), out sectorLength) ? sectorLength : 0,
                                Offset = int.TryParse((string)sector.Attribute("Offset"), out offset) ? offset : 0
                            });
                        }

                        int platformLength;
                        platform = new Platform
                        {
                            Name = (string)plat.Attribute("Name"),
                            Length = int.TryParse((string)plat.Attribute("Length"), out platformLength) ? platformLength : 0,
                            WhereFrom = !string.IsNullOrWhiteSpace(whereFrom) ? new Station { NameRu = whereFrom } : null,
                            WhereTo = !string.IsNullOrWhiteSpace(whereTo) ? new Station { NameRu = whereTo } : null,
                            Sectors = sectors
                        };
                    }

                    var path = new Pathways
                    {
                        Id = int.Parse((string)directXml.Attribute("Id")),
                        Name = (string)directXml.Attribute("Name"),
                        НаНомерПуть = (string)directXml.Attribute("НаНомерПуть"),
                        НаНомерОмПути = (string)directXml.Attribute("НаНомерОмПути"),
                        СНомерОгоПути = (string)directXml.Attribute("СНомерОгоПути"),
                        Addition = (string)directXml.Attribute("Addition"),
                        Addition2 = (string)directXml.Attribute("Addition2"),
                        Platform = platform
                    };

                    pathWays.Add(path);
                }
            }
            catch (Exception ex)
            {
                Library.Logs.Log.log.Error($"Ошибка при парсинге путей: {ex}");
            }

            return pathWays;
        }



        public IEnumerable<Pathways> List(Expression<Func<Pathways, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public void Add(Pathways entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(Pathways entity)
        {
            throw new NotImplementedException();
        }

        public void Edit(Pathways entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(Expression<Func<Pathways, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public void AddRange(IEnumerable<Pathways> entity)
        {
            throw new NotImplementedException();
        }
    }
}