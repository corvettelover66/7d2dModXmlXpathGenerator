using SevenDaysToDieModCreator.Extensions;
using System;
using System.IO;
using System.Text;
using System.Xml;

namespace SevenDaysToDieModCreator.Models
{
    class ModInfo
    {

        public bool ModInfoExists { get; private set; }
        private const string MOD_INFO_NAME_TAG = "Name";
        private const string MOD_INFO_DESCRIPTION_TAG = "Description";
        private const string MOD_INFO_AUTHOR_TAG = "Author";
        private const string MOD_INFO_VERSION_TAG = "Version";

        public static string MOD_INFO_FILE_NAME = "ModInfo.xml";
        public static string ModInfoFilePath { get => Path.Combine(XmlFileManager._ModDirectoryOutputPath, MOD_INFO_FILE_NAME); } 

        public ModInfo() 
        {
            if (File.Exists(ModInfoFilePath))
            {
                bool didSucceed = LoadSettingsFromFile();
                ModInfoExists = didSucceed;
                if (!didSucceed)
                {
                    XmlFileManager.WriteStringToLog("Failed Loading mod info.");
                }
            }
            else 
            {
                File.Create(ModInfoFilePath);
                ModInfoExists = false;
            }
        }
        public ModInfo(string name, string description, string author, string version)
        {
            Name = name;
            Description = description;
            Author = author;
            Version = version;
            if (!File.Exists(ModInfoFilePath))
            {
                File.Create(ModInfoFilePath);
            }
        }

        public string Name 
        {
            get => Properties.Settings.Default.ModInfoName;
            private set
            { 
                Properties.Settings.Default.ModInfoName = value;
                Properties.Settings.Default.Save();
            }
        }
        public string Description 
        {
            get => Properties.Settings.Default.ModInfoDescription;
            private set
            {
                Properties.Settings.Default.ModInfoDescription = value;
                Properties.Settings.Default.Save();
            }
        }
        public string Author
        {
            get => Properties.Settings.Default.ModInfoAuthor;
            private set
            {
                Properties.Settings.Default.ModInfoAuthor = value;
                Properties.Settings.Default.Save();
            }
        }
        public string Version
        {
            get => Properties.Settings.Default.ModInfoVersion;
            private set
            {
                Properties.Settings.Default.ModInfoVersion = value;
                Properties.Settings.Default.Save();
            }
        }
        public bool LoadSettingsFromFile() 
        {
            bool didSucceed = false;
            XmlDocument xmlDocument = new XmlDocument();
            try
            {
                xmlDocument.Load(ModInfoFilePath);
                XmlNodeList allModInfoNodes = xmlDocument.GetElementsByTagName("ModInfo");
                XmlNode firstChild = allModInfoNodes.Item(0);
                if (firstChild != null) 
                {
                    foreach (XmlNode nextNode in firstChild.ChildNodes) 
                    {
                        if (nextNode.Name.Equals(MOD_INFO_NAME_TAG)) this.Name = nextNode.GetAvailableAttribute().Value;
                        else if (nextNode.Name.Equals(MOD_INFO_DESCRIPTION_TAG)) this.Description = nextNode.GetAvailableAttribute().Value;
                        else if (nextNode.Name.Equals(MOD_INFO_AUTHOR_TAG)) this.Author = nextNode.GetAvailableAttribute().Value;
                        else if (nextNode.Name.Equals(MOD_INFO_VERSION_TAG)) this.Version = nextNode.GetAvailableAttribute().Value;
                    }
                didSucceed = true;
                }
            }
            catch (Exception e)
            {
                XmlFileManager.WriteStringToLog(e.Message);
            }
            return didSucceed;
        }
        
        override public string ToString() 
        {
            StringBuilder modInfoBuilder = new StringBuilder();
            modInfoBuilder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
            modInfoBuilder.AppendLine("<xml>");
            modInfoBuilder.AppendLine("\t<ModInfo>");
            modInfoBuilder.AppendLine("\t\t<Name value=\"" + this.Name + "\" />");
            modInfoBuilder.AppendLine("\t\t<Description value=\"" + this.Description + "\" />");
            modInfoBuilder.AppendLine("\t\t<Author value=\"" + this.Author + "\" />");
            modInfoBuilder.AppendLine("\t\t<Version value=\"" + this.Version + "\" />");
            modInfoBuilder.AppendLine("\t</ModInfo>");
            modInfoBuilder.AppendLine("</xml>");
            return modInfoBuilder.ToString();
        }
    }
}
