using System.IO;
using Amver.Domain.Constants;
using NLog;

namespace Amver.WebApi
{
    public static class InitializeLogs
    {
        public static void Init()
        {
            LogManager.Configuration.Variables["pathToLog"] = GlobalVariables.Log;
            LogManager.Configuration.Variables["directorySeparator"] = GlobalVariables.DirectorySeparatorCharacter.ToString();
        }
    }
}