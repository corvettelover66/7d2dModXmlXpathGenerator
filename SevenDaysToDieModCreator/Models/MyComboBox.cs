using SevenDaysToDieModCreator.Controllers;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace SevenDaysToDieModCreator.Models
{
    class MyComboBox : ComboBox
    {
        public MyComboBox(XmlNode ObjectNode,  bool isGameFileSearchTRee = true)
        {
            this.IsGameFileSearchTree = isGameFileSearchTRee;
            this.ObjectNode = ObjectNode;
        }

        public TextBox MyTextBox { get; private set; }
        public bool IsGameFileSearchTree { get; }

        public XmlNode ObjectNode { get; private set; }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var textBox = Template.FindName("PART_EditableTextBox", this) as TextBox;
            MyTextBox = textBox;
            MyTextBox.Tag = this.Parent;
            MyTextBox.Uid = ObjectNode == null ? "" : ObjectNode.Name;
            XmlObjectsListWrapper wrapperToUse = MainWindowViewController.LoadedListWrappers.GetValueOrDefault(this.Uid);
            if (IsGameFileSearchTree) TreeViewGenerator.AddTargetContextMenuToControl(MyTextBox, wrapperToUse);
            MyTextBox.Background = BackgroundColorController.GetBackgroundColor();
            if(!this.Resources.Contains(SystemColors.WindowBrushKey)) this.Resources.Add(SystemColors.WindowBrushKey, BackgroundColorController.GetBackgroundColor());
        }
    }
}
