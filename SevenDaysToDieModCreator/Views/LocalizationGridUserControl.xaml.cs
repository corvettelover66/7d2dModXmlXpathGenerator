using SevenDaysToDieModCreator.Extensions;
using SevenDaysToDieModCreator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        private Dictionary<int, List<TextBox>> TextBoxRowDictionary;
        public LocalizationGridUserControl(string pathToFile, bool IsSingleRecord = false) 
        {
            InitializeComponent();
            LocalizationFileObject = new LocalizationFileObject(pathToFile);
            TextBoxRowDictionary = new Dictionary<int, List<TextBox>>();
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
            foreach (string headerField in LocalizationFileObject.HeaderValuesMap.Keys)
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
                nextHeaderTextBox = new TextBox() { Text = headerField, FontSize = 22, Tag ="," };
                Grid.SetRow(nextHeaderTextBox, row);
                Grid.SetColumn(nextHeaderTextBox, columnCount);
                topGrid.Children.Add(nextHeaderTextBox);
                columnCount++;
                topGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                nextHeaderTextBox = new TextBox() { Text = nextField, FontSize = 22, Tag = "," };
                Grid.SetRow(nextHeaderTextBox, row);
                Grid.SetColumn(nextHeaderTextBox, columnCount);
                topGrid.Children.Add(nextHeaderTextBox);
            }
            if (nextHeaderTextBox != null) nextHeaderTextBox.Tag = "\n";
        }
        public void AddEmptyRow(XmlObjectsListWrapper selectedModItemsWrapper, XmlObjectsListWrapper selectedModBlocksWrapper) 
        {
            int lastRowPlusOne = TextBoxRowDictionary.Keys.Count + 1;
            List<TextBox> rowToMimic = TextBoxRowDictionary.GetValueOrDefault(TextBoxRowDictionary.Keys.Count - 1);

            RowDefinition rowDefinition = new RowDefinition();
            topGrid.RowDefinitions.Add(rowDefinition);
            int columnCount = 0;
            AddNumberColumn(lastRowPlusOne, columnCount);
            columnCount++;
            AddClearButton(lastRowPlusOne, columnCount);
            columnCount++;
            AddModKeysColumn(lastRowPlusOne, columnCount, selectedModItemsWrapper, selectedModBlocksWrapper);
            //Remove the keys column from the row to mimic as we are filling that in with a special combo box
            if(rowToMimic != null && rowToMimic.Count >0)rowToMimic.RemoveAt(rowToMimic.Count - 1);
            columnCount++;
            List<string> emptyRecord = new List<string>();
            for(int textBoxCount = 0; textBoxCount < LocalizationFileObject.HeaderValuesMap.Keys.Count; textBoxCount++) 
            {
                //This means there was not a row to mimic, increase the count by one to avoid that.
                if (rowToMimic == null && textBoxCount == 0) textBoxCount++;
                emptyRecord.Add("");
            }
            AddFieldColumns(lastRowPlusOne, columnCount, emptyRecord);
        }
        private void GenerateLocalizationGrid()
        {
            topGrid.HorizontalAlignment = HorizontalAlignment.Left;
            topGrid.VerticalAlignment = VerticalAlignment.Top;
            topGrid.Background = new SolidColorBrush(Colors.LightGreen);
            CreateHeaderRow();
            CreateLocalizationRecordsGrid();
        }
        private void CreateLocalizationRecordsGrid()
        {
            //Set row to one as first row is taken by the headers
            int rowCount = 1;
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
        private void CreateHeaderRow()
        {
            int row = 0;

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
            foreach (string headerField in LocalizationFileObject.HeaderValuesMap.Keys)
            {
                //Set the Headers
                topGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                nextHeaderTextBox = new TextBox() { Text = headerField, FontSize = 22 };
                Grid.SetRow(nextHeaderTextBox, row);
                Grid.SetColumn(nextHeaderTextBox, columnCount);
                topGrid.Children.Add(nextHeaderTextBox);
                nextHeaderTextBox.Tag = ",";
                columnCount++;
            }
            if(nextHeaderTextBox != null)nextHeaderTextBox.Tag = "\n";
        }
        private void AddDummyColumn(int row, int columnCount, string textForTextBox = "", int fontSize = 18)
        {
            //Add a dummy column for the clear button.
            topGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            TextBox emptyTextBox = new TextBox() { Text = textForTextBox, FontSize = fontSize, };
            Grid.SetRow(emptyTextBox, row);
            Grid.SetColumn(emptyTextBox, columnCount);
            topGrid.Children.Add(emptyTextBox);
        }
        private void AddNumberColumn(int rowCount, int column)
        {
            int numberForColumn = rowCount + 1;
            //Need to increase the row count to make sure the numbers line up with the file
            topGrid.ColumnDefinitions.Add(new ColumnDefinition());
            TextBox clearRowButton = new TextBox { Text = numberForColumn+ "", FontSize = 18 };
            Grid.SetRow(clearRowButton, rowCount);
            Grid.SetColumn(clearRowButton, column);
            topGrid.Children.Add(clearRowButton);
        }
        private void AddClearButton(int rowCount, int columnCount)
        {
            //Add the first column, the clear button
            topGrid.ColumnDefinitions.Add(new ColumnDefinition());
            Button clearRowButton = new Button { Content = "Clear row", FontSize = 18, Tag = rowCount };
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
                List<TextBox> textBoxRow = TextBoxRowDictionary.GetValueOrDefault(row);
                TextBox lastBox = null;
                foreach (TextBox nextBox in textBoxRow)
                {
                    nextBox.Text = "";
                    nextBox.Tag = "";
                    lastBox = nextBox;
                }
                if (lastBox != null) lastBox.Tag = "\n";
            }
        }
        private void AddFieldColumns(int rowCount, int startingColumn, List<string> record)
        {
            List<TextBox> allBoxesInRow = new List<TextBox>();
            int columnCount = startingColumn;
            //Each field
            TextBox newRecordTextbox = null;
            int count = 0;
            foreach (string nextRecordField in record)
            {
                topGrid.ColumnDefinitions.Add(new ColumnDefinition());
                newRecordTextbox = new TextBox()
                {
                    Text = nextRecordField,
                    Tag = ",",
                    FontSize = 18
                };
                if(count < this.LocalizationFileObject.HeaderValues.Length) newRecordTextbox.AddToolTip(this.LocalizationFileObject.HeaderValues[count]);
                newRecordTextbox.TextChanged += NewRecordTextbox_TextChanged;
                newRecordTextbox.LostFocus += NewRecordTextbox_LostFocus;
                Grid.SetRow(newRecordTextbox, rowCount);
                Grid.SetColumn(newRecordTextbox, columnCount);
                topGrid.Children.Add(newRecordTextbox);
                allBoxesInRow.Add(newRecordTextbox);
                columnCount++;
                count++;
            }
            if(newRecordTextbox != null)newRecordTextbox.Tag = "\n";
            if (allBoxesInRow.Count > 0) TextBoxRowDictionary.Add(rowCount, allBoxesInRow);
        }

        private void NewRecordTextbox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox recordTextBox) 
            {
                if (String.IsNullOrEmpty(recordTextBox.Text)) recordTextBox.Tag = "";
                else recordTextBox.Tag = ",";
            }
        }

        private void NewRecordTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.GridHasChanged = true;
        }
        private void AddModKeysColumn(int lastRowPlusOne, int columnCount, XmlObjectsListWrapper selectedModItemsWrapper, XmlObjectsListWrapper selectedModBlocksWrapper)
        {
            topGrid.ColumnDefinitions.Add(new ColumnDefinition());
            ComboBox newCommonValuesBox = new ComboBox();
            newCommonValuesBox.FontSize = 18;
            newCommonValuesBox.IsEditable = true;
            newCommonValuesBox.AddToolTip("key");
            newCommonValuesBox.DropDownClosed += NewCommonValuesBox_DropDownClosed;
            newCommonValuesBox.LostFocus += NewCommonValuesBox_LostFocus;
            if (selectedModItemsWrapper != null) 
            {
                Dictionary<string, List<string>> attributeDictinaryForItems = selectedModItemsWrapper.objectNameToAttributeValuesMap.GetValueOrDefault("item");
                if (attributeDictinaryForItems != null) 
                {
                    List<string> commonAttributes = attributeDictinaryForItems.GetValueOrDefault("name");
                    commonAttributes.Insert(0, "");
                    newCommonValuesBox.SetComboBox(commonAttributes);
                }            
            }
            if (selectedModBlocksWrapper != null) 
            { 
                Dictionary<string, List<string>> attributeDictinaryForBlocks = selectedModBlocksWrapper.objectNameToAttributeValuesMap.GetValueOrDefault("block");
                if (attributeDictinaryForBlocks != null)
                {
                    List<string> commonAttributes = attributeDictinaryForBlocks.GetValueOrDefault("name");
                    newCommonValuesBox.SetComboBox(commonAttributes);
                }
            }
            Grid.SetRow(newCommonValuesBox, lastRowPlusOne);
            Grid.SetColumn(newCommonValuesBox, columnCount);
            topGrid.Children.Add(newCommonValuesBox);
        }

        private void NewCommonValuesBox_DropDownClosed(object sender, EventArgs e)
        {
            if (sender is ComboBox modKeyComboBox) 
            {
                if (String.IsNullOrEmpty(modKeyComboBox.Text)) modKeyComboBox.Tag = "";
                else
                {
                    modKeyComboBox.Tag = ",";
                    GridHasChanged = true;
                }
            }
        }

        private void NewCommonValuesBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox modKeyComboBox) 
            {
                if (String.IsNullOrEmpty(modKeyComboBox.Text)) modKeyComboBox.Tag = "";
                else
                {
                    modKeyComboBox.Tag = ",";
                    GridHasChanged = true;
                }
            }
        }
    }
}
