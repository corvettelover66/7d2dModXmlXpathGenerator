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
    public partial class LocalizationView : UserControl
    {
        private LocalizationFileObject LocalizationFileObject;
        public Grid Maingrid { get; private set; }

        private Dictionary<int, List<TextBox>> TextBoxRowDictionary;
        public LocalizationView(string pathToFile)
        {
            InitializeComponent();
            LocalizationFileObject = new LocalizationFileObject(pathToFile);
            TextBoxRowDictionary = new Dictionary<int, List<TextBox>>();
            GenerateLocalizationGrid();
            this.Maingrid = topGrid;
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
        private void AddFieldColumns(int rowCount, int startingColumn, List<string> record)
        {
            List<TextBox> allBoxesInRow = new List<TextBox>();
            int columnCount = startingColumn;
            //Each field
            TextBox newRecordTextbox = null;
            foreach (string nextRecordField in record)
            {
                topGrid.ColumnDefinitions.Add(new ColumnDefinition());
                newRecordTextbox = new TextBox()
                {
                    Text = nextRecordField,
                    FontSize = 16
                };
                newRecordTextbox.Tag = ",";
                Grid.SetRow(newRecordTextbox, rowCount);
                Grid.SetColumn(newRecordTextbox, columnCount);
                topGrid.Children.Add(newRecordTextbox);
                allBoxesInRow.Add(newRecordTextbox);
                columnCount++;
            }
            if(newRecordTextbox != null)newRecordTextbox.Tag = "\n";
            if (allBoxesInRow.Count > 0) TextBoxRowDictionary.Add(rowCount, allBoxesInRow);
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
        private void AddDummyColumn(int row, int columnCount, string textForTextBox = "")
        {
            //Add a dummy column for the clear button.
            topGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            TextBox emptyTextBox = new TextBox() { Text = textForTextBox, FontSize = 18, };
            Grid.SetRow(emptyTextBox, row);
            Grid.SetColumn(emptyTextBox, columnCount);
            topGrid.Children.Add(emptyTextBox);
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
    }
}
