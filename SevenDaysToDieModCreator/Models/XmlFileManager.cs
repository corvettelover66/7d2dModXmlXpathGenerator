using System;
using System.Collections.Generic;
using System.IO;

namespace SevenDaysToDieModCreator.Models
{
    class XmlFileManager
    {
        private static string ReadFileContents;
        public static string _fileOutputPath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "Output/");
        public static string _ModPath
        {
            get => Properties.Settings.Default.ModTagSetting + "/Config/";
            set => _ModPath = value;
        }
        public static string _ModOutputPath
        {
            get => Path.Combine(Directory.GetCurrentDirectory(), "Output/Mods/" + Properties.Settings.Default.ModTagSetting + "/Config/");
            set => _ModOutputPath = value;
        }
        public static string _LoadedFilesPath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "Game_XMLS/");
        public static string Xui_Folder_Name = "XUi";
        public static string Xui_Menu_Folder_Name = "XUi_Menu";
        private static readonly string logFileName = "log.txt";
        public static string Get_ModOutputPath(string customTagName)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "Output/Mods/" + customTagName + "/Config/");
        }
        public static void WriteStringToFile(string filePath, string fileName, string stringToWrite, bool addTimeStamp = false)
        {
            if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
            if (addTimeStamp && !String.IsNullOrEmpty(stringToWrite)) stringToWrite = "<!-- Xml Written " + DateTime.Now.ToString("MMMM dd, yyyy") + " at " + DateTime.Now.ToString("HH:mm:ss") + " -->\n" + stringToWrite;
            try
            {
                System.IO.File.WriteAllText(Path.Combine(filePath, fileName), stringToWrite);
            }
            catch (IOException)
            {
                WriteStringToLog("ERROR Writing to file @" + filePath + fileName);
            }
        }
        public static List<string> GetCustomModFoldersInOutput()
        {
            if (!Directory.Exists(_fileOutputPath + "/Mods/")) Directory.CreateDirectory(_fileOutputPath + "/Mods/");
            string[] allDirs = Directory.GetDirectories(_fileOutputPath + "/Mods/", "*");
            List<string> justChildrenPathNames = new List<string>();
            foreach (string nextDir in allDirs)
            {
                justChildrenPathNames.Add(Path.GetFileName(nextDir));
            }
            return justChildrenPathNames;
        }
        public static List<string> GetCustomModFilesInOutput(string customTag, string prefix = "")
        {
            List<string> allModFiles = new List<string>();
            string customModFilesInOutputDirectory = Get_ModOutputPath(customTag);
            Directory.CreateDirectory(customModFilesInOutputDirectory);
            string[] allXmlsInOutputPath = Directory.GetFiles(customModFilesInOutputDirectory, "*.xml");
            AddFilesToList(allModFiles, allXmlsInOutputPath, prefix);
            if (Directory.Exists(Path.Combine(customModFilesInOutputDirectory, Xui_Folder_Name)))
            {
                string[] xuiFiles = Directory.GetFiles(Path.Combine(customModFilesInOutputDirectory, Xui_Folder_Name));
                if (xuiFiles.Length > 0) AddFilesToList(allModFiles, xuiFiles, prefix + Xui_Folder_Name + "_");

            }
            if (Directory.Exists(Path.Combine(customModFilesInOutputDirectory, Xui_Menu_Folder_Name)))
            {
                string[] xuiMenuFiles = Directory.GetFiles(Path.Combine(customModFilesInOutputDirectory, Xui_Menu_Folder_Name));
                if (xuiMenuFiles.Length > 0) AddFilesToList(allModFiles, xuiMenuFiles, prefix + Xui_Menu_Folder_Name + "_");
            }
            //Check for Xui menu files
            return allModFiles;
        }
        private static void AddFilesToList(List<string> allModFiles, string[] allXmlsInOutputPath, string filePrefix = "")
        {
            foreach (string nextFile in allXmlsInOutputPath)
            {
                allModFiles.Add(filePrefix + Path.GetFileName(nextFile.Substring(0, nextFile.Length - 4)));
            }
        }
        public static void WriteStringToLog(string xml, bool addTimeStamp = true)
        {
            if (!Directory.Exists(_fileOutputPath)) Directory.CreateDirectory(_fileOutputPath);
            string filePath = _fileOutputPath + logFileName;
            if (!File.Exists(filePath)) CreateFilePath(_fileOutputPath, logFileName);

            if (addTimeStamp) xml = "<!-- Written " + DateTime.Now.ToString("MMMM dd, yyyy") + " at " + DateTime.Now.ToString("HH:mm:ss") + " -->\n" + xml;

            AppendToFile(_fileOutputPath, logFileName, xml);
        }
        public static string GetFileContents(string path, string fileName)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            string filePath = Path.Combine(path, fileName);
            if (File.Exists(filePath)) ReadFile(filePath);
            else ReadFileContents = "";
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
        private static void ReadFile(string path)
        {
            string line = null;
            try
            {
                using StreamReader sr = new StreamReader(@path);
                line = sr.ReadToEnd();
                if (line.Length < 1) line = null;
            }
            catch (FileNotFoundException)
            {
                WriteStringToLog("ERROR Reading file @" + @path);
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
        internal static bool CheckLoadedModFolderForXmlFiles(string fullSelectedPath)
        {
            string modConfigPath = Path.Combine(fullSelectedPath, "config");
            string[] filesInConfig = Directory.GetFiles(modConfigPath);
            string[] directoriesInConfig = Directory.GetDirectories(modConfigPath);
            return CheckDirectoryForXmlFiles(filesInConfig, directoriesInConfig);
        }
        internal static void CopyAllFilesToPath(string inputPath, string outputPath, bool doOverwriteFiles = false)
        {
            string[] filesInInputPath = Directory.GetFiles(inputPath);
            string[] directoriesInInputPath = Directory.GetDirectories(inputPath);

            CopyFilesRecursive(inputPath, outputPath, filesInInputPath, directoriesInInputPath, doOverwriteFiles);
        }
        private static void CopyFilesRecursive(string inputPath, string outputPath, string[] filesInDirectory, string[] directoriesInConfig, bool doOverwriteFiles = false)
        {
            Directory.CreateDirectory(outputPath);
            foreach (string fileName in filesInDirectory)
            {
                string newFilePath = Path.Combine(outputPath, Path.GetFileName(fileName));
                if (Path.GetFileName(fileName).Contains(".xml"))
                {
                    if (doOverwriteFiles)
                    {
                        if (File.Exists(newFilePath)) File.Delete(newFilePath);
                        File.Copy(fileName, newFilePath);
                    }
                    else if (!File.Exists(newFilePath)) File.Copy(fileName, newFilePath);
                }
            }
            if (directoriesInConfig.Length > 0)
            {
                foreach (string directory in directoriesInConfig)
                {
                    string[] filesInSubDir = Directory.GetFiles(directory);
                    string[] directoriesInSubDir = Directory.GetDirectories(directory);
                    if (filesInSubDir.Length > 0 || directoriesInSubDir.Length > 0)
                    {
                        string newDirectoryOutputPath = Path.Combine(outputPath, Path.GetFileName(directory));
                        string newDirectoryInputPath = Path.Combine(inputPath, Path.GetFileName(directory));
                        CopyFilesRecursive(newDirectoryInputPath, newDirectoryOutputPath, filesInSubDir, directoriesInSubDir, doOverwriteFiles);
                    }
                }
            }
        }
        private static bool CheckDirectoryForXmlFiles(string[] filesInDirectory, string[] directoriesInConfig = null)
        {
            bool hasXmlFiles = false;
            if (directoriesInConfig != null)
            {
                foreach (string directory in directoriesInConfig)
                {
                    string[] filesInSubDir = Directory.GetFiles(directory);
                    if (filesInSubDir.Length > 0) hasXmlFiles = CheckDirectoryForXmlFiles(filesInSubDir);
                    if (hasXmlFiles) break;
                }
            }
            foreach (string fileName in filesInDirectory)
            {
                if (hasXmlFiles) break;
                if (Path.GetFileName(fileName).Contains(".xml"))
                {
                    hasXmlFiles = true;
                    break;
                }
            }
            return hasXmlFiles;
        }
        public static void CopyAllOutputFiles()
        {
            string gameModDirectory = Properties.Settings.Default.GameFolderModDirectory + _ModPath;
            Directory.CreateDirectory(gameModDirectory);
            Directory.CreateDirectory(_ModOutputPath);
            if (!String.IsNullOrEmpty(gameModDirectory)) CopyAllFilesToPath(_ModOutputPath, gameModDirectory, true);
        }
        internal static void RenameModDirectory(string oldModName, string newModName)
        {
            string oldModDirectory = Path.Combine(_fileOutputPath, "Mods", oldModName);
            string newModDirectory = Path.Combine(_fileOutputPath, "Mods", newModName);
            Directory.Move(oldModDirectory, newModDirectory);
        }
    }
}
