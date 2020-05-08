using SevenDaysToDieModCreator.Controllers;

using System.Windows.Controls;
using System.Xml;

namespace SevenDaysToDieModCreator.Models
{
    class MyComboBox : ComboBox
    {
        public MyComboBox(ObjectViewController objectViewController, bool isGameFileSearchTRee = true)
        {
            ObjectViewController = objectViewController;
            this.IsGameFileSearchTree = isGameFileSearchTRee;
        }

        public TextBox MyTextBox { get; private set; }
        public ObjectViewController ObjectViewController { get; }
        public bool IsGameFileSearchTree { get; }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var textBox = Template.FindName("PART_EditableTextBox", this) as TextBox;
            MyTextBox = textBox;
            MyTextBox.Tag = this.Parent;
            if (IsGameFileSearchTree) this.ObjectViewController.AddTargetContextMenuToControl(MyTextBox);
        }
    }
}
