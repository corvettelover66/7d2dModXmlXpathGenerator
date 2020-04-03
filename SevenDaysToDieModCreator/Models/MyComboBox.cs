using SevenDaysToDieModCreator.Controllers;

using System.Windows.Controls;

namespace SevenDaysToDieModCreator.Models
{
    class MyComboBox : ComboBox
    {
        public MyComboBox(ObjectViewController objectViewController, bool addContextMenu = true)
        {
            ObjectViewController = objectViewController;
            this.DoAddContextMenu = addContextMenu;
        }

        public TextBox MyTextBox { get; private set; }
        public ObjectViewController ObjectViewController { get; }
        public bool DoAddContextMenu { get; }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var textBox = Template.FindName("PART_EditableTextBox", this) as TextBox;
            MyTextBox = textBox;
            MyTextBox.Tag = this.Parent;
            if (DoAddContextMenu) this.ObjectViewController.AddTargetContextMenuToControl(MyTextBox);
        }
    }
}
