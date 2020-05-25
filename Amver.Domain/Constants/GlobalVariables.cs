using System;
using System.IO;

namespace Amver.Domain.Constants
{
    public static class GlobalVariables
    {
        public static readonly string DataRoot = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        
        public static readonly char DirectorySeparatorCharacter = Path.DirectorySeparatorChar;
        
        public const string ApplicationName = "Amver";
        
        public static string Log => Path.Combine(DataRoot, ApplicationName, "log");
        
        public static string ImageContent = "image/jpeg";
    }
}