using SevenDaysToDieModCreator.Controllers;
using SevenDaysToDieModCreator.Extensions;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SevenDaysToDieModCreator.Models
{
    class MyComboBox : ComboBox
    {
        public MyComboBox(ObjectViewController objectViewController)
        {
            ObjectViewController = objectViewController;
        }

        public TextBox MyTextBox { get; private set; }
        public ObjectViewController ObjectViewController { get; }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var textBox = Template.FindName("PART_EditableTextBox", this) as TextBox;
            MyTextBox = textBox;
            MyTextBox.Tag = this.Parent;
            this.ObjectViewController.AddContextMenuToControl(MyTextBox);
        }
    }
}
