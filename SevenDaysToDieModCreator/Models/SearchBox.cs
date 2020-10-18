using SevenDaysToDieModCreator.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace SevenDaysToDieModCreator.Models
{
    class SearchBox : ComboBox
    {
        private const string NAME = "SearchTextBox";
        public SearchBox(SortedSet<string> valuesToPopulateBox) 
        {
            this.IsEditable = true;
            List<string> valuesToPopulateBoxList = new List<string>(valuesToPopulateBox);
            this.SetComboBox(valuesToPopulateBoxList);
            this.Name = NAME;
        }
    }
}
