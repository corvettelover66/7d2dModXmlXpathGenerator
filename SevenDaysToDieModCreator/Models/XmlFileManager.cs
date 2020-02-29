using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SevenDaysToDieModCreator.Models
{
    class XmlFileManager
    {
        private static string ReadFileContents;
        public static string _filePath = Path.Combine(Directory.GetCurrentDirectory(), "Output/");
        public static string _ModPath = Path.Combine(Directory.GetCurrentDirectory(), "Output/Mods/Your Mod Name/config/");
        public static string LOCAL_DIR = Path.Combine(Directory.GetCurrentDirectory(), "Game_XMLS/");
        private static string logFileName = Path.Combine(Directory.GetCurrentDirectory(), "log.txt");

        public static void WriteStringToFile(string filePath, string fileName, string stringToWrite, bool addTimeStamp = false)
        {
            if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
            //WriteXmlToLog(stringToWrite, fileName);
            if (addTimeStamp && !String.IsNullOrEmpty(stringToWrite)) stringToWrite ="<!-- Xml Written at " + DateTime.Now.ToString("yyyy:m:d h:mm:ss tt") + " -->\n" + stringToWrite ;
            try
            {
                System.IO.File.WriteAllText(@filePath + fileName, stringToWrite);
            }
            catch (IOException) 
            {
                WriteXmlToLog("ERROR Writing to file @" + @filePath + fileName);
            }
        }
        public static void WriteXmlToLog(string xml, string filename = null, bool addTimeStamp = true)
        {
            if (!Directory.Exists(@_filePath)) Directory.CreateDirectory(@_filePath);
            if (addTimeStamp) xml = "<!-- Xml Written at " + DateTime.Now.ToString("yyyy:mm:dd h:mm:ss tt") + " -->\n" + xml;
            string stringToWrite = "";
            if (filename == null) stringToWrite = xml;
            else stringToWrite = "<!-- For file: " + filename + "-->\n" + xml;
            AppendToFile(@_filePath, logFileName, stringToWrite);
        }
        public static string GetFileContents(string path, string fileName)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            string filePath = @path + fileName;
            if (!File.Exists(filePath)) CreateFileModPath(path, fileName);
            ReadFile(path, fileName);
            return ReadFileContents;
        }
        private static void ReadFile(string path, string fileName)
        {
            string line = null;
            try
            {
                using (StreamReader sr = new StreamReader(@path + fileName))
                {
                    line = sr.ReadToEnd();
                    if (line.Length < 1) line = null;
                }
            }
            catch (FileNotFoundException)
            {
                WriteXmlToLog("ERROR Reading file @"+ @path + fileName);
            }
            ReadFileContents = line;
        }
        private static void CreateFileModPath(string path, string fileName)
        {
            try
            {
                using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(@path + fileName, true))
                {
                    file.Write("");
                }
            }
            catch (IOException)
            {
                WriteXmlToLog("ERROR Creating file @" + @path + fileName);
            }
        }
        private static void AppendToFile(string path, string fileName, string stringToWrite)
        {
            try {
                using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(@path + fileName, true))
                {
                    file.WriteLine(stringToWrite);
                }
            }
            catch (IOException exce)
            {
            }
        }
    }
}
