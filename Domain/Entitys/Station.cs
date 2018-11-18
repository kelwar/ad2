namespace Domain.Entitys
{
    public class Station : EntityBase
    {
        public string NameRu { get; set; }
        public string NameEng { get; set; }
        public string NameCh { get; set; }
        public int CodeEsr { get; set; }
        public int CodeExpress { get; set; }
        public string NearestStation { get; set; }
        

        public override string ToString()
        {
            return NameRu;
        }

        public override bool Equals(object obj)
        {
            var station = obj as Station;
            return station != null &&
                   //CodeEsr != 0 &&
                   CodeEsr == station.CodeEsr &&
                   CodeExpress == station.CodeExpress &&
                   NameRu == station.NameRu;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}