using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;

namespace SevenDaysToDieModCreator.Models
{
    public class LocalizationFileObject
    {
        public bool LOCALIZATION_EXIST { get; private set; }
        public bool PARSING_ERROR { get; private set; } = false;


        public const string LOCALIZATION_FILE_NAME = "Localization.txt";
        public string KeyColumn { get; set; }
        
        private readonly string[] DefaultHeaderColumns = new string[]{ "Key", "Source", "Context", "Changes", "english", "german", "latam", "french", "italian", "japanese", "koreana", "polish", "brazilian", "russian", "turkish", "schinese", "tchinese", "spanish" };
        //Does  hard check on the passed in headerToCheck string to see if it is a "file" or "Source column because the game file uses a different 
        //nomenclature for the same header when creating mod localizations"
        public static bool HeaderIsFileOrSourceColumn(string headerToCheck) 
        {
            return headerToCheck.ToLower().Equals("source") || headerToCheck.ToLower().Equals("file") ;
        }
        //Does  hard check on the passed in headerToCheck string to see if it is a "type" or "context" column because the game file uses a different 
        //nomenclature for the same header when creating mod localizations"
        public static bool HeaderIsTypeOrContextColumn(string headerToCheck)
        {
            return headerToCheck.ToLower().Equals("type") || headerToCheck.ToLower().Equals("context");

        }
        public string[] HeaderValues { get; private set; }
        //A dictionary of all value columns by the header as the key
        //Key: Header from the CSV
        //Value: A list of all values from the file for that header.
        public Dictionary<string, SortedSet<string>> HeaderKeyToCommonValuesMap { get; private set; }
        //A dictionary of each record in the localization file that is found using the Key column.
        //Key: The "Key" column in the Localizationtxt file
        //Value: The record of the key.
        public Dictionary<string, List<string>> KeyValueToRecordMap { get; private set; }


        //A list of each record in the CSV, the records are stored individually as another list.
        public List<List<string>> RecordList { get; private set; }
        public LocalizationFileObject(string pathToFile)
        {
            HeaderKeyToCommonValuesMap = new Dictionary<string, SortedSet<string>>();
            KeyValueToRecordMap = new Dictionary<string, List<string>>();
            RecordList = new List<List<string>>();
            HeaderValues = DefaultHeaderColumns;
            LOCALIZATION_EXIST = File.Exists(pathToFile);
            if (LOCALIZATION_EXIST)
            {
                try
                {
                    TraverseLocalizatonFile(pathToFile);
                }
                catch (Exception e) 
                {
                    XmlFileManager.WriteStringToLog("Localization parsing error: \n\n" + e.StackTrace);
                    PARSING_ERROR = true;
                }
            }
            else
            {
                foreach (string header in DefaultHeaderColumns)
                {
                    HeaderKeyToCommonValuesMap.Add(header, new SortedSet<string>());
                }
            }
        }
        //THROWS Index OUTOFRnge EXception if the ROW count does not match the Header count
        private void TraverseLocalizatonFile(string pathToFile)
        {
            using TextFieldParser parser = new TextFieldParser(@pathToFile)
            {
                HasFieldsEnclosedInQuotes = true
            };
            parser.SetDelimiters(",");
            int rowCount = 0;
            while (!parser.EndOfData)
            {
                //Processing row
                string[] fields = parser.ReadFields();
                if (rowCount == 0) HeaderValues = new string[fields.Length];
                List<string> newRecord = new List<string>();
                int columnCount = 0;
                foreach (string field in fields)
                {
                    //Set new lists for the HeaderValuesMap as we're going through all the keys on the first pass.
                    if (rowCount == 0)
                    {
                        //Get the first column to ensure there is no funny buisness
                        if (columnCount == 0) this.KeyColumn = field.ToLower();
                        HeaderValues[columnCount] = field.ToLower();
                        HeaderKeyToCommonValuesMap.Add(field.ToLower(), new SortedSet<string>());
                    }
                    //Set the record list
                    else
                    {
                        if (columnCount == 0 && !KeyValueToRecordMap.ContainsKey(field)) KeyValueToRecordMap.Add(field, newRecord);
                        //Add 
                        //Add the field to the new record 
                        newRecord.Add(field);
                        //Add the field value to the appropriate list in the header map.
                        if(columnCount < HeaderValues.Length) HeaderKeyToCommonValuesMap.GetValueOrDefault(HeaderValues[columnCount]).Add(field);
                    }
                    columnCount++;
                }
                //Add the new record to the list
                if(newRecord.Count > 0) RecordList.Add(newRecord);
                rowCount++;
            }
        }
    }
}
