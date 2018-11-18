using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Logs
{
    class ActionLog
    {
        public static void CarNavigationLog(string Сообщение, string userName)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(File.Open("Logs\\CarNavigation\\" + DateTime.Now.ToString("yy.MM.dd") + ".log", FileMode.Append)))
                {
                    sw.WriteLine(DateTime.Now.ToString("HH:mm:ss") + ": \t" + (userName ?? "ЦИС") + ": \t" + "Повагонная навигация" + "\t" + Сообщение);
                    sw.Close();
                }
            }
            catch (Exception ex) { };
        }
    }
}
