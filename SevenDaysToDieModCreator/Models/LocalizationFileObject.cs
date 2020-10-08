using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SevenDaysToDieModCreator.Models
{
    public class LocalizationFileObject
    {
        public bool LOCALIZATION_EXIST { get; private set; }
        public const string LOCALIZATION_FILE_NAME = "Localization.txt";
        public string KeyColumn { get; set; }
        
        private string[] DefaultHeaderColumns = new string[]{ "Key", "Source", "Context", "Changes", "english", "german", "latam", "french", "italian", "japanese", "koreana", "polish", "brazilian", "russian", "turkish", "schinese", "tchinese", "spanish" };

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
            LOCALIZATION_EXIST = File.Exists(pathToFile);
            if (LOCALIZATION_EXIST) TraverseLocalizatonFile(pathToFile);
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
            using TextFieldParser parser = new TextFieldParser(@pathToFile);
            parser.HasFieldsEnclosedInQuotes = true;
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
