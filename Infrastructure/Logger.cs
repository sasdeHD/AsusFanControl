using AsusFanControl.Model.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsusFanControl.Infrastructure
{
    public class Logger
    {
        private static object _lock = new object();

        public static void Log(string message, LogLevel logLevel = LogLevel.Error)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                path += Path.DirectorySeparatorChar;
            }

            if (!path.EndsWith("\\"))
            {
                path += "\\";
            }

            lock (_lock)
            {
                System.IO.Directory.CreateDirectory(path);
                var now = DateTime.Now;
                var dateInFileName = now.Date.ToString("yyyy-MM-dd");

                using (var file = System.IO.File.AppendText(path + "Asus_Control_" + dateInFileName + ".log"))
                {
                    file.WriteLine("[" + logLevel.ToDescriptionString() + "] " + now.ToShortDateString() + " " + now.TimeOfDay.ToString("c") + ": " + message);
                    file.Flush();
                }
            }
        }
    }
}
