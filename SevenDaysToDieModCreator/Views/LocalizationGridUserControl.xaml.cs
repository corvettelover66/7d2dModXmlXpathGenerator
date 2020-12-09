using SevenDaysToDieModCreator.Controllers;
using SevenDaysToDieModCreator.Extensions;
using SevenDaysToDieModCreator.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SevenDaysToDieModCreator.Views
{
    /// <summary>
    /// Interaction logic for LocalizationView.xaml
    /// </summary>
    public partial class LocalizationGridUserControl : UserControl
    {
        public LocalizationFileObject LocalizationFileObject { get; private set; }
        public Grid Maingrid { get; private set; }
        public bool GridHasChanged { get; private set; }
        public void SetGridChangedToFalse() 
        {
            this.GridHasChanged = false;
        }
        public void SetGridChangedToTrue()
        {
            this.GridHasChanged = true;
        }
        
        private readonly Dictionary<int, List<Control>> TextBoxRowDictionary;
        public LocalizationGridUserControl(string pathToFile, bool IsSingleRecord = false) 
        {
            InitializeComponent();
            LocalizationFileObject = new LocalizationFileObject(pathToFile);
            TextBoxRowDictionary = new Dictionary<int, List<Control>>();
            if (IsSingleRecord) GenerateSigleRecordGrid();
            else GenerateLocalizationGrid();
            this.Maingrid = topGrid;
            GridHasChanged = false;
        }

        private void GenerateSigleRecordGrid()
        {
            topGrid.HorizontalAlignment = HorizontalAlignment.Left;
            topGrid.VerticalAlignment = VerticalAlignment.Top;
            topGrid.Background = new SolidColorBrush(Colors.LightGreen);
            CreateSingleRecordGrid();
        }
        private void CreateSingleRecordGrid()
        {
            RowDefinition rowDefinition = new RowDefinition();
            int row = 0;
            topGrid.RowDefinitions.Add(rowDefinition);
            int numberForLineCountColumn = row + 1;
            int columnCount = 0;
            AddDummyColumn(row, columnCount, numberForLineCountColumn + "");
            columnCount++;
            AddDummyColumn(row, columnCount,  "Header", 22);
            columnCount++;
            AddDummyColumn(row, columnCount, "Value", 22);
            List<string> record = null;
            if(this.LocalizationFileObject.RecordList.Count > 0 ) record = this.LocalizationFileObject.RecordList[0];
            TextBox nextHeaderTextBox = null;
            foreach (string headerField in LocalizationFileObject.HeaderKeyToCommonValuesMap.Keys)
            {
                columnCount = 0;
                //Go throh the keys for the header row
                rowDefinition = new RowDefinition();
                //Make sure the record is retrieved before increasing the row.
                string nextField = record[row];
                row++;
                topGrid.RowDefinitions.Add(rowDefinition);
                numberForLineCountColumn = row + 1;
                AddDummyColumn(row, columnCount, numberForLineCountColumn + "");
                columnCount++;
                //Set the Headers
                topGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                nextHeaderTextBox = new TextBox() { 
                    Text = headerField, 
                    FontSize = 22, 
                    Tag =row,
                    Background = BackgroundColorController.GetBackgroundColor() };
                Grid.SetRow(nextHeaderTextBox, row);
                Grid.SetColumn(nextHeaderTextBox, columnCount);
                topGrid.Children.Add(nextHeaderTextBox);
                columnCount++;
                topGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                nextHeaderTextBox = new TextBox() { 
                    Text = nextField, FontSize = 22, 
                    Tag = row,
                    Background = BackgroundColorController.GetBackgroundColor() };
                Grid.SetRow(nextHeaderTextBox, row);
                Grid.SetColumn(nextHeaderTextBox, columnCount);
                topGrid.Children.Add(nextHeaderTextBox);
            }
        }
        public void AddEmptyRow(List<XmlObjectsListWrapper> allWrappersToUseForKeys, LocalizationFileObject gameLocalizationFile) 
        {
            int lastRowPlusOne = TextBoxRowDictionary.Keys.Count + 1;
            List<Control> controlsAddedInRow = new List<Control>();
            RowDefinition rowDefinition = new RowDefinition();
            topGrid.RowDefinitions.Add(rowDefinition);
            int columnCount = 0;
            AddNumberColumn(lastRowPlusOne, columnCount);
            columnCount++;
            AddClearButton(lastRowPlusOne, columnCount);
            columnCount++;
            AddModKeysColumn(lastRowPlusOne, columnCount, allWrappersToUseForKeys);
            columnCount++;
            //In the game file it is file, in the mod file it is source
            string headerKey = "file";
            controlsAddedInRow.Add( AddGameFileColumn(lastRowPlusOne, columnCount, headerKey, gameLocalizationFile));
            columnCount++;
            headerKey = "type";
            controlsAddedInRow.Add(AddGameFileColumn(lastRowPlusOne, columnCount, headerKey, gameLocalizationFile));
            columnCount++;
            controlsAddedInRow.Add(AddChangesColumn(lastRowPlusOne, columnCount));
            columnCount++;
            List<string> emptyRecord = new List<string>();
            //Count the Headers to set an empty row
            for(int textBoxCount = 0; textBoxCount < LocalizationFileObject.HeaderKeyToCommonValuesMap.Keys.Count; textBoxCount++) 
            {
                emptyRecord.Add("");
            }
            //This is used to skip the first 4 headers
            int skipHeadersCount = 4;
            AddFieldColumns(lastRowPlusOne, columnCount, emptyRecord, controlsAddedInRow, skipHeadersCount);
        }
        private void GenerateLocalizationGrid()
        {
            topGrid.HorizontalAlignment = HorizontalAlignment.Left;
            topGrid.VerticalAlignment = VerticalAlignment.Top;
            topGrid.Background = new SolidColorBrush(Colors.LightGreen);
            int rowCount = 0;
            CreateHeaderRow(rowCount);
            rowCount++;
            //CreateSearchBoxRow(rowCount);
            //rowCount++;
            CreateLocalizationRecordsGrid(rowCount);
        }
        private void CreateSearchBoxRow(int rowCount)
        {
            RowDefinition rowDefinition = new RowDefinition();
            topGrid.RowDefinitions.Add(rowDefinition);
            int colunnCount = 0;
            foreach (string nextHeader in LocalizationFileObject.HeaderValues) 
            {
                AddSearchBoxColumn(colunnCount, nextHeader);
            }
        }
        private void AddSearchBoxColumn(int colunnCount, string nextHeader)
        {
            topGrid.ColumnDefinitions.Add(new ColumnDefinition());

            //TextBox clearRowButton = new TextBox { Text = numberForColumn + "", FontSize = 18 };
            //Grid.SetRow(clearRowButton, rowCount);
            //Grid.SetColumn(clearRowButton, column);
            //topGrid.Children.Add(clearRowButton);
        }
        private void CreateLocalizationRecordsGrid(int rowCount)
        {
            //Set row to one as first row is taken by the headers
            //go through all of the records
            foreach (List<string> record in LocalizationFileObject.RecordList)
            {
                //Start the row for the record
                RowDefinition rowDefinition = new RowDefinition();
                topGrid.RowDefinitions.Add(rowDefinition);
                AddNumberColumn(rowCount, 0);
                AddClearButton(rowCount, 1);
                AddFieldColumns(rowCount, 2, record);
                rowCount++;                
            }
        }
        private void CreateHeaderRow(int row)
        {
            //Go throh the keys for the header row
            RowDefinition rowDefinition = new RowDefinition();
            topGrid.RowDefinitions.Add(rowDefinition);
            
            int columnCount = 0;
            int numberForLineCountColumn = row + 1;
            AddDummyColumn(row, columnCount, numberForLineCountColumn + "");
            columnCount++;
            AddDummyColumn(row, columnCount);
            columnCount++;
            TextBox nextHeaderTextBox = null;
            foreach (string headerField in LocalizationFileObject.HeaderKeyToCommonValuesMap.Keys)
            {
                //Set the Headers
                topGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                nextHeaderTextBox = new TextBox() { 
                    Text = headerField, 
                    FontSize = 22, 
                    Tag = row,
                    Background = BackgroundColorController.GetBackgroundColor() };
                Grid.SetRow(nextHeaderTextBox, row);
                Grid.SetColumn(nextHeaderTextBox, columnCount);
                topGrid.Children.Add(nextHeaderTextBox);

                columnCount++;
            }
        }
        private void AddDummyColumn(int row, int columnCount, string textForTextBox = "", int fontSize = 18)
        {
            //Add a dummy column for the clear button.
            topGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            TextBox emptyTextBox = new TextBox() { 
                Text = textForTextBox, 
                FontSize = fontSize, 
                Background = BackgroundColorController.GetBackgroundColor() 
            };
            Grid.SetRow(emptyTextBox, row);
            Grid.SetColumn(emptyTextBox, columnCount);
            topGrid.Children.Add(emptyTextBox);
        }
        private void AddNumberColumn(int rowCount, int column)
        {
            int numberForColumn = rowCount + 1;
            //Need to increase the row count to make sure the numbers line up with the file
            topGrid.ColumnDefinitions.Add(new ColumnDefinition());
            TextBox clearRowButton = new TextBox { 
                Text = numberForColumn+ "",
                FontSize = 18, 
                Background = BackgroundColorController.GetBackgroundColor() };
            Grid.SetRow(clearRowButton, rowCount);
            Grid.SetColumn(clearRowButton, column);
            topGrid.Children.Add(clearRowButton);
        }
        private void AddClearButton(int rowCount, int columnCount)
        {
            //Add the first column, the clear button
            topGrid.ColumnDefinitions.Add(new ColumnDefinition());
            Button clearRowButton = new Button { Content = "Clear row", Tag = rowCount, FontSize = 18, Background = BackgroundColorController.GetBackgroundColor() };
            clearRowButton.Click += ClearRowButton_Click;
            Grid.SetRow(clearRowButton, rowCount);
            Grid.SetColumn(clearRowButton, columnCount);
            topGrid.Children.Add(clearRowButton);
        }
        private void ClearRowButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button clearRowButtonSender)
            {
                int row = Int32.Parse(clearRowButtonSender.Tag.ToString());
                List<Control> textBoxRow = TextBoxRowDictionary.GetValueOrDefault(row);
                foreach (Control nextControl in textBoxRow)
                {
                    if (nextControl is ComboBox controlAsCombobox) 
                    {
                        controlAsCombobox.Text = "";
                    }
                    if (nextControl is TextBox controlAsTextBox) 
                    {
                        controlAsTextBox.Text = "";
                    }
                }
            }
        }
        private void AddFieldColumns(int rowCount, int startingColumn, List<string> record, List<Control> allBoxesInRow = null, int skipHeadersCount = 0)
        {
            if(allBoxesInRow == null)allBoxesInRow = new List<Control>();
            int columnCount = startingColumn;
            //Each field
            foreach (string nextRecordField in record)
            {
                ColumnDefinition columnDefinition = new ColumnDefinition();
                topGrid.ColumnDefinitions.Add(new ColumnDefinition());
                TextBox newRecordTextbox = new TextBox()
                {
                    Text = nextRecordField,
                    Tag = rowCount,
                    FontSize = 18,
                    Background = BackgroundColorController.GetBackgroundColor()
                };
                if(skipHeadersCount < this.LocalizationFileObject.HeaderValues.Length) newRecordTextbox.AddToolTip((rowCount +1) + " : " +this.LocalizationFileObject.HeaderValues[skipHeadersCount]);
                newRecordTextbox.TextChanged += NewRecordTextbox_TextChanged;
                Grid.SetRow(newRecordTextbox, rowCount);
                Grid.SetColumn(newRecordTextbox, columnCount);
                topGrid.Children.Add(newRecordTextbox);
                allBoxesInRow.Add(newRecordTextbox);
                columnCount++;
                skipHeadersCount++;
            }
            if (allBoxesInRow.Count > 0) TextBoxRowDictionary.Add(rowCount, allBoxesInRow);
        }
        private void NewRecordTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.GridHasChanged = true;
        }
        private Control AddModKeysColumn(int lastRowPlusOne, int columnCount, List<XmlObjectsListWrapper> xmlWrappersForGameKeys)
        {
            topGrid.ColumnDefinitions.Add(new ColumnDefinition());
            ComboBox newCommonValuesBox = new ComboBox
            {
                FontSize = 18,
                Tag = lastRowPlusOne,
                IsEditable = true,
                Background = BackgroundColorController.GetBackgroundColor()
            };
            newCommonValuesBox.Resources.Add(SystemColors.WindowBrushKey, BackgroundColorController.GetBackgroundColor());
            newCommonValuesBox.AddToolTip("key");
            newCommonValuesBox.DropDownClosed += NewCommonValuesBox_DropDownClosed;
            newCommonValuesBox.LostFocus += NewCommonValuesBox_LostFocus;
            AddModAttributesFromWrappers(xmlWrappersForGameKeys, newCommonValuesBox);
            Grid.SetRow(newCommonValuesBox, lastRowPlusOne);
            Grid.SetColumn(newCommonValuesBox, columnCount);
            topGrid.Children.Add(newCommonValuesBox);
            return newCommonValuesBox;
        }

        private void AddModAttributesFromWrappers(List<XmlObjectsListWrapper> xmlWrappersForGameKeys, ComboBox newCommonValuesBox)
        {
            //Go through all mod files
            foreach (XmlObjectsListWrapper xmlObjectsListWrapper in xmlWrappersForGameKeys)
            {
                if (xmlObjectsListWrapper != null)
                {
                    foreach (string tagNameKey in xmlObjectsListWrapper.ObjectNameToAttributeValuesMapNoXpath.Keys)
                    {
                        //Get the next top tag attribute mao
                        Dictionary<string, List<string>> attributeDictinaryForItems = xmlObjectsListWrapper.ObjectNameToAttributeValuesMapNoXpath.GetValueOrDefault(tagNameKey);
                        if (attributeDictinaryForItems != null)
                        {
                            //Get the first key in the attribute dictinary because we only want the first attribute
                            //Probably a better way than a loop and break
                            foreach (string key in attributeDictinaryForItems.Keys)
                            {
                                List<string> commonAttributes = attributeDictinaryForItems.GetValueOrDefault(key);
                                if (commonAttributes != null)
                                {
                                    newCommonValuesBox.AddUniqueValueTo(commonAttributes);
                                    //We only want the name, or key attribute. Essentially the first attribute in the wrapper dictionary.
                                    break;
                                }
                            }
                        }
                        //We only want the first tag found.
                        break;
                    }
                }
            }
        }

        private Control AddGameFileColumn(int lastRowPlusOne, int columnCount, string headerKey, LocalizationFileObject gameLocalizationFile)
        {
            SortedSet<string> allCommonValuesSorted = gameLocalizationFile.HeaderKeyToCommonValuesMap.GetValueOrDefault(headerKey);
            List<string> allCommonValues = new List<string>(allCommonValuesSorted);

            if (allCommonValues == null) allCommonValues = new List<string>();
            topGrid.ColumnDefinitions.Add(new ColumnDefinition());
            ComboBox newCommonValuesBox = new ComboBox
            {
                FontSize = 18,
                Tag = lastRowPlusOne,
                IsEditable = true,
                Background = BackgroundColorController.GetBackgroundColor()
            };
            newCommonValuesBox.Resources.Add(SystemColors.WindowBrushKey, BackgroundColorController.GetBackgroundColor());
            newCommonValuesBox.AddToolTip(headerKey);
            newCommonValuesBox.DropDownClosed += NewCommonValuesBox_DropDownClosed;
            newCommonValuesBox.LostFocus += NewCommonValuesBox_LostFocus;
            newCommonValuesBox.SetComboBox(allCommonValues);
            Grid.SetRow(newCommonValuesBox, lastRowPlusOne);
            Grid.SetColumn(newCommonValuesBox, columnCount);
            topGrid.Children.Add(newCommonValuesBox);
            return newCommonValuesBox;
        }
        private Control AddChangesColumn(int lastRowPlusOne, int columnCount)
        {
            List<string> changesColumn = new List<string>() { "", "New" }; 
            topGrid.ColumnDefinitions.Add(new ColumnDefinition());
            ComboBox newCommonValuesBox = new ComboBox
            {
                FontSize = 18,
                Tag = lastRowPlusOne,
                IsEditable = true,
                Background = BackgroundColorController.GetBackgroundColor()
            };
            newCommonValuesBox.Resources.Add(SystemColors.WindowBrushKey, BackgroundColorController.GetBackgroundColor());

            newCommonValuesBox.AddToolTip("changes");
            newCommonValuesBox.DropDownClosed += NewCommonValuesBox_DropDownClosed;
            newCommonValuesBox.LostFocus += NewCommonValuesBox_LostFocus;
            newCommonValuesBox.SetComboBox(changesColumn);
            Grid.SetRow(newCommonValuesBox, lastRowPlusOne);
            Grid.SetColumn(newCommonValuesBox, columnCount);
            topGrid.Children.Add(newCommonValuesBox);
            return newCommonValuesBox;
        }
        private void NewCommonValuesBox_DropDownClosed(object sender, EventArgs e)
        {
            if (sender is ComboBox modKeyComboBox) 
            {
                if (!String.IsNullOrEmpty(modKeyComboBox.Text)) GridHasChanged = true;
            }
        }
        private void NewCommonValuesBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox modKeyComboBox) 
            {
                if (!String.IsNullOrEmpty(modKeyComboBox.Text)) GridHasChanged = true;
            }
        }
    }
}
