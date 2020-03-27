using System;
using System.Collections.Generic;
using System.IO;

namespace SevenDaysToDieModCreator.Models
{
    class XmlFileManager
    {
        private static string ReadFileContents;
        public static string _filePath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "Output/");
        //public static string _ModPath { get; set; } = Properties.Settings.Default.CustomTagName + "/config/";
        public static string _ModPath
        {
            get => Properties.Settings.Default.CustomTagName + "/config/";
            set => _ModPath = value;
        }
        public static string _ModOutputPath
        {
            get => Path.Combine(Directory.GetCurrentDirectory(), "Output/Mods/" + Properties.Settings.Default.CustomTagName + "/config/");
            set => _ModOutputPath = value;
        }
        public static string _LoadedFilesPath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "Game_XMLS/");
        private static readonly string logFileName =  "log.txt";
        public static string Get_ModOutputPath(string customTagName) 
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "Output/Mods/" + customTagName + "/config/");
        }
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
                WriteStringToLog("ERROR Writing to file @" + @filePath + fileName);
            }
        }
        public static List<string> GetCustomModFoldersInOutput()
        {
            if (!Directory.Exists(@_filePath + "/Mods/")) Directory.CreateDirectory(@_filePath + "/Mods/");
            string[] allDirs = Directory.GetDirectories(@_filePath + "/Mods/", "*");
            List<string> justChildrenPathNames = new List<string>();
            foreach (string nextDir in allDirs)
            {
                justChildrenPathNames.Add(Path.GetFileName(nextDir));
            }
            return justChildrenPathNames;
        }
        public static List<string> GetCustomModFilesInOutput(string customTag, string filePrefix = "")
        {
            List<string> allModFiles = new List<string>();
            string customModFilesInOutputDirectory = Get_ModOutputPath(customTag);
            if (!Directory.Exists(customModFilesInOutputDirectory)) Directory.CreateDirectory(customModFilesInOutputDirectory);
            foreach (string nextFile in Directory.GetFiles(customModFilesInOutputDirectory, "*.xml")) 
            {
                allModFiles.Add(filePrefix + Path.GetFileName(nextFile.Substring(0, nextFile.Length - 4)));
            }
            return allModFiles;
        }
        public static void WriteStringToLog(string xml, bool addTimeStamp = true)
        {
            if (!Directory.Exists(@_filePath)) Directory.CreateDirectory(@_filePath);
            string filePath = @_filePath + logFileName;
            if (!File.Exists(filePath)) CreateFilePath(@_filePath, logFileName);

            if (addTimeStamp) xml = "<!-- Written " + DateTime.Now.ToString("MMMM dd, yyyy") +" at " + DateTime.Now.ToString("HH:mm:ss") + " -->\n" + xml;

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
            string fileContents = GetFileContents(_ModOutputPath, fileName);
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
                WriteStringToLog("ERROR Reading file @"+ @path + fileName);
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
                //WriteStringToLog("ERROR Creating file @" + @path + fileName);
            }
        }
        private static void AppendToFile(string path, string fileName, string stringToWrite)
        {
            using System.IO.StreamWriter file = new System.IO.StreamWriter(@path + fileName, true);
            file.WriteLine(stringToWrite);
        }

        public static void CopyAllOutputFiles()
        {
            string gameModDirectory = Properties.Settings.Default.GameFolderModDirectory + _ModPath;
            if (!Directory.Exists(gameModDirectory)) Directory.CreateDirectory(gameModDirectory);
            if(!Directory.Exists(_ModOutputPath)) Directory.CreateDirectory(_ModOutputPath);
            if (!String.IsNullOrEmpty(gameModDirectory)) 
            {
                foreach (string nextFile in Directory.GetFiles(_ModOutputPath, "*.xml"))
                {
                    string gameModDirectoryNextFile = gameModDirectory + Path.GetFileName(nextFile);
                    if (File.Exists(gameModDirectoryNextFile)) File.Delete(gameModDirectoryNextFile);
                    File.Copy(nextFile, gameModDirectoryNextFile);
                }
            }
        }
        public static bool IsDirectory(string path)
        {
            bool isDirectory = false;
            if (Directory.Exists(path)) isDirectory = true; // is a directory 
            return isDirectory;
        }
    }
}
