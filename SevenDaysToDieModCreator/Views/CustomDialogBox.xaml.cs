using SevenDaysToDieModCreator.Extensions;
using System.Windows;

namespace SevenDaysToDieModCreator.Views
{
    /// <summary>
    /// Interaction logic for CustomDialogBox.xaml
    /// </summary>
    public partial class CustomDialogBox : Window
    {
        public CustomDialogBox()
        {
            InitializeComponent();
            InputTextBlock.AddOnHoverMessage("Example: ThatGuyJonesysNewMod " );
            LabelTextBlock.Text = "Thank you for downloading the 7 days to die Mod Creator! " +
                "Please input your custom tag now! This will be the top tag used in the xml output files. " +
                "IMPORTANT: If you lose work check the log.txt in the Output folder. " +
                "Any time you close the app or reset the object view, the xml that could be generated is output in that log. " +
                "If you like the mod don't forget to drop a like and tell your friends!";
        }

        public string ResponseText
        {
            get { return InputTextBlock.Text; }
            set { InputTextBlock.Text = value; }
        }

        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
