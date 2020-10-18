using System;
using System.Collections.Generic;
using System.IO;

namespace SevenDaysToDieModCreator.Models
{
    class XmlFileManager
    {
        private static string ReadFileContents;
        public static string FileOutputPath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "Output/");
        public static string ModDirectoryOutputPath
        {
            get => Path.Combine(Directory.GetCurrentDirectory(), "Output/Mods/", Properties.Settings.Default.ModTagSetting);
            set => ModDirectoryOutputPath = value;
        }
        public static string ModConfigOutputPath
        {
            get => Path.Combine(Directory.GetCurrentDirectory(), "Output/Mods/" + Properties.Settings.Default.ModTagSetting + "/Config/");
            set => ModConfigOutputPath = value;
        }
        public static string LoadedFilesPath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "Game_XMLS/");
        public static string Xui_Folder_Name = "XUi";

        internal static void ReplaceTagsInModFiles(string oldCustomTag, string newCustomTag)
        {
            string customModFilesInOutputDirectory = Get_ModOutputPath(newCustomTag);
            Directory.CreateDirectory(customModFilesInOutputDirectory);
            string[] allXmlsInOutputPath = Directory.GetFiles(customModFilesInOutputDirectory, "*.xml");
            TraverseFilesForReplace(allXmlsInOutputPath, oldCustomTag, newCustomTag);

            if (Directory.Exists(Path.Combine(customModFilesInOutputDirectory, Xui_Folder_Name)))
            {
                string[] xuiFiles = Directory.GetFiles(Path.Combine(customModFilesInOutputDirectory, Xui_Folder_Name));
                if (xuiFiles.Length > 0) 
                {
                    TraverseFilesForReplace(xuiFiles, oldCustomTag, newCustomTag);
                }
            }
            if (Directory.Exists(Path.Combine(customModFilesInOutputDirectory, Xui_Menu_Folder_Name)))
            {
                string[] xuiMenuFiles = Directory.GetFiles(Path.Combine(customModFilesInOutputDirectory, Xui_Menu_Folder_Name));
                if (xuiMenuFiles.Length > 0) 
                {
                    TraverseFilesForReplace(xuiMenuFiles, oldCustomTag, newCustomTag);
                }
            }
        }

        private static void TraverseFilesForReplace(string[] allXmlsInOutputPath, string oldCustomTag, string newCustomTag)
        {
            foreach (string nextFilePath in allXmlsInOutputPath)
            {
                string fileName = Path.GetFileName(nextFilePath);
                string contents = GetFileContents(nextFilePath.Replace(fileName, ""), fileName);
                contents = contents.Replace("<" +oldCustomTag +">", "<" + newCustomTag + ">");
                contents = contents.Replace("</" + oldCustomTag + ">", "</" + newCustomTag + ">");
                WriteStringToFile(nextFilePath.Replace(fileName, ""), fileName, contents);
            }
        }

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
            if (!Directory.Exists(FileOutputPath + "/Mods/")) Directory.CreateDirectory(FileOutputPath + "/Mods/");
            string[] allDirs = Directory.GetDirectories(FileOutputPath + "/Mods/", "*");
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
                allModFiles.Add(filePrefix + Path.GetFileName(nextFile[0..^4]));
            }
        }
        public static void WriteStringToLog(string xml, bool addTimeStamp = true)
        {
            if (!Directory.Exists(FileOutputPath)) Directory.CreateDirectory(FileOutputPath);
            string filePath = FileOutputPath + logFileName;
            if (!File.Exists(filePath)) CreateFilePath(FileOutputPath, logFileName);

            if (addTimeStamp) xml = "<!-- Written " + DateTime.Now.ToString("MMMM dd, yyyy") + " at " + DateTime.Now.ToString("HH:mm:ss") + " -->\n" + xml;

            AppendToFile(FileOutputPath, logFileName, xml);
        }
        public static string GetFileContents(string path, string fileName)
        {
            if (!Directory.Exists(path) && !String.IsNullOrEmpty(path)) Directory.CreateDirectory(path);
            string filePath = Path.Combine(path, fileName);
            if (File.Exists(filePath)) ReadFile(filePath);
            else ReadFileContents = "";
            return ReadFileContents;
        }
        //Pass in the Custom Tag to exclude it from the read.
        public static string ReadExistingFile(string fileName, string MyCustomTagName = null)
        {
            string fileContents = GetFileContents(ModConfigOutputPath, fileName);
            if (fileContents != null && MyCustomTagName != null)
            {
                fileContents = fileContents.Replace("<" + MyCustomTagName + ">", "");
                fileContents = fileContents.Replace("</" + MyCustomTagName + ">", "");
            }

            return fileContents;
        }
        private static void ReadFile(string path)
        {
            FileInfo fileInfo;
            StreamReader streamReader =null;
            string line = null;
            try
            {
                fileInfo = new FileInfo(path);
                streamReader = fileInfo.OpenText();
                line = streamReader.ReadToEnd();
                if (line.Length < 1) line = null;
            }
            catch (FileNotFoundException)
            {
                WriteStringToLog("File not found ERROR Reading file @" + @path);
            }
            catch (UnauthorizedAccessException)
            {
                WriteStringToLog("Unauthorized access ERROR Reading file @" + @path);
            }
            catch (Exception) 
            {
                WriteStringToLog("ERROR Reading file @" + @path);
            }
            if(streamReader != null) streamReader.Close();
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
                if (doOverwriteFiles)
                {
                    if (File.Exists(newFilePath)) File.Delete(newFilePath);
                    File.Copy(fileName, newFilePath);
                }
                else if (!File.Exists(newFilePath)) File.Copy(fileName, newFilePath);
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
            string gameModDirectory = Path.Combine(Properties.Settings.Default.GameFolderModDirectory, Properties.Settings.Default.ModTagSetting);

            Directory.CreateDirectory(gameModDirectory);
            Directory.CreateDirectory(ModConfigOutputPath);
            if (!String.IsNullOrEmpty(gameModDirectory)) CopyAllFilesToPath(ModDirectoryOutputPath, gameModDirectory, true);
        }
        internal static void RenameModDirectory(string oldModName, string newModName)
        {
            string tempDirName = "temp";
            string oldModDirectory = Path.Combine(FileOutputPath, "Mods", oldModName);
            string newModDirectory = Path.Combine(FileOutputPath, "Mods", newModName);
            string tempModDirectory = Path.Combine(FileOutputPath, "Mods", tempDirName);
            //Handle edge case where directory names are the same when ignoreing case but they are actually different when taking in Case. Windows ignores the case and I don't want to.
            if (oldModName.ToLower().Equals(newModName.ToLower())
                && !oldModName.Equals(newModName))
            {
                Directory.Move(oldModDirectory, tempModDirectory);
                Directory.Move(tempModDirectory, newModDirectory);
            }
            else 
            {
                Directory.Move(oldModDirectory, newModDirectory);
            }
        }
        internal static void DeleteModFile(string modTagSetting, string fileName)
        {
            try
            {
                string fullPath = Path.Combine(Get_ModOutputPath(modTagSetting), fileName);
                if (File.Exists(fullPath)) 
                {
                    File.Delete(fullPath);
                    WriteStringToLog("Deleted file " + fullPath);
                }
            }
            catch (IOException e) 
            {
                WriteStringToLog(e.Message);
            }
        }
    }
}
