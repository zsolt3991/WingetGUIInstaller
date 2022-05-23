using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WingetGUIInstaller.Constants
{
    public static class LoggingConstants
    {
        public const string LogFileName = "wingetguiinstallerlog.log";
        public const string LogTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level}] [{SourceContext}] {Message}{NewLine}{Exception}";
    }
}
