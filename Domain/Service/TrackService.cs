using Domain.Abstract;
using Domain.Concrete;
using Domain.Entitys;
using Library.Logs;
using Library.Xml;
using System;
using System.IO;
using System.Windows.Forms;

namespace Domain.Service
{
    public class TrackService
    {
        private IRepository<Pathways> _trackRepository;
        private string _filePath;

        public TrackService(string filePath)
        {
            _trackRepository = Load(filePath);
            if (_trackRepository == null)
                Application.Exit();
        }

        private IRepository<Pathways> Load(string filePath)
        {
            try
            {
                var xmlFile = XmlWorker.LoadXmlFile(filePath); //все настройки в одном файле
                if (xmlFile == null)
                    throw new FileNotFoundException("Файл PathNames.xml не найден или не соответствует формату xml. Откорректируйте файл и повторите попытку");

                return new RepositoryXmlPathways(xmlFile);
            }
            catch (Exception ex)
            {
                Log.log.Error(ex);
                MessageBox.Show($"файл \"PathNames.xml\" не загружен. Исключение: {ex.Message}");
            }

            return null;
        }
    }
}
