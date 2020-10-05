using SevenDaysToDieModCreator.Extensions;
using SevenDaysToDieModCreator.Models;
using System;
using System.Collections.Generic;
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
        public LocalizationView(string pathToFile)
        {
            InitializeComponent();
            LocalizationFileObject = new LocalizationFileObject(pathToFile);
            GenerateLocalizationGrid();
            this.Maingrid = topGrid;
        }

        private void GenerateLocalizationGrid()
        {
            topGrid.HorizontalAlignment = HorizontalAlignment.Left;
            topGrid.VerticalAlignment = VerticalAlignment.Top;
            topGrid.Background = new SolidColorBrush(Colors.LightGreen);
            //Go throh the keys for the header row
            RowDefinition rowDefinition = new RowDefinition();
            topGrid.RowDefinitions.Add(rowDefinition);
            int columnCount = 0;
            int rowCount = 1;
            foreach (string headerField in LocalizationFileObject.HeaderValuesMap.Keys) 
            {
                //Set the Headers
                topGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                TextBox nextHeaderTextBox = new TextBox() { Text = headerField, FontSize=22};
                Grid.SetRow(nextHeaderTextBox, rowCount);
                Grid.SetColumn(nextHeaderTextBox, columnCount);
                topGrid.Children.Add(nextHeaderTextBox);
                columnCount++;
                if (columnCount == LocalizationFileObject.HeaderValuesMap.Keys.Count) nextHeaderTextBox.Tag = "\n";
                else nextHeaderTextBox.Tag = ",";
            }
            //go through all of the records
            foreach (List<string> record in LocalizationFileObject.RecordList) 
            {
                rowCount++;
                rowDefinition = new RowDefinition();
                topGrid.RowDefinitions.Add(rowDefinition);
                columnCount = 0;
                //Each fields
                foreach (string nextRecordField in record) 
                {
                    topGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    TextBox newRecordTextbox = new TextBox()
                    {
                        Text = nextRecordField,
                        FontSize = 16,
                        TextWrapping = TextWrapping.Wrap
                    };
                    Grid.SetRow(newRecordTextbox, rowCount);
                    Grid.SetColumn(newRecordTextbox, columnCount);
                    topGrid.Children.Add(newRecordTextbox);
                    columnCount++;
                    if (columnCount == record.Count) newRecordTextbox.Tag = "\n";
                    else newRecordTextbox.Tag = ",";
                }
            }
        }
    }
}
