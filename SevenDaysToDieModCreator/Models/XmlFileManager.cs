using System;
using System.IO;

namespace SevenDaysToDieModCreator.Models
{
    class XmlFileManager
    {
        private static string ReadFileContents;
        public static string _filePath = Path.Combine(Directory.GetCurrentDirectory(), "Output/");
        public static string _ModPath = Path.Combine(Directory.GetCurrentDirectory(), "Output/Mods/Your Mod Name/config/");
        public static string LOCAL_DIR = Path.Combine(Directory.GetCurrentDirectory(), "Game_XMLS/");
        private static readonly string logFileName =  "log.txt";

        public static void WriteStringToFile(string filePath, string fileName, string stringToWrite, bool addTimeStamp = false)
        {
            if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
            if (addTimeStamp && !String.IsNullOrEmpty(stringToWrite)) stringToWrite ="<!-- Xml Written " + DateTime.Now.ToString("MMMM dd, yyyy") + " at " + DateTime.Now.ToString("HH:mm:ss") + " -->\n" + stringToWrite ;
            try
            {
                System.IO.File.WriteAllText(@filePath + fileName, stringToWrite);
            }
            catch (IOException) 
            {
                WriteXmlToLog("ERROR Writing to file @" + @filePath + fileName);
            }
        }
        public static void WriteXmlToLog(string xml, bool addTimeStamp = true)
        {
            if (!Directory.Exists(@_filePath)) Directory.CreateDirectory(@_filePath);
            string filePath = @_filePath + logFileName;
            if (!File.Exists(filePath)) CreateFilePath(@_filePath, logFileName);

            if (addTimeStamp) xml = "<!-- Xml Written " + DateTime.Now.ToString("MMMM dd, yyyy") +" at " + DateTime.Now.ToString("HH:mm:ss") + " -->\n" + xml;

            AppendToFile(@_filePath, logFileName, xml);
        }
        public static string GetFileContents(string path, string fileName)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            string filePath = @path + fileName;
            if (!File.Exists(filePath)) CreateFilePath(path, fileName);
            ReadFile(path, fileName);
            return ReadFileContents;
        }
        //Pass in the Custom Tag to exclude it from the read.
        public static string ReadExistingFile(string fileName, string MyCustomTagName = null)
        {
            string fileContents = GetFileContents(@_ModPath, fileName);
            if (fileContents != null && MyCustomTagName != null)
            {
                fileContents = fileContents.Replace("<" + MyCustomTagName + ">\n", "");
                fileContents = fileContents.Replace("</" + MyCustomTagName + ">", "");
            }

            return fileContents;
        }
        private static void ReadFile(string path, string fileName)
        {
            string line = null;
            try
            {
                using StreamReader sr = new StreamReader(@path + fileName);
                line = sr.ReadToEnd();
                if (line.Length < 1) line = null;
            }
            catch (FileNotFoundException)
            {
                WriteXmlToLog("ERROR Reading file @"+ @path + fileName);
            }
            ReadFileContents = line;
        }
        private static void CreateFilePath(string path, string fileName)
        {
            try
            {
                using System.IO.StreamWriter file = new System.IO.StreamWriter(@path + fileName, true);
                file.Write("");
            }
            catch (IOException)
            {
                WriteXmlToLog("ERROR Creating file @" + @path + fileName);
            }
        }
        private static void AppendToFile(string path, string fileName, string stringToWrite)
        {
            using System.IO.StreamWriter file = new System.IO.StreamWriter(@path + fileName, true);
            file.WriteLine(stringToWrite);
        }
    }
}
