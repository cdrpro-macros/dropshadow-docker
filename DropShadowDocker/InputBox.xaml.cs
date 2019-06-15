using System.Windows;

namespace DropShadow
{
    public partial class InputBox : Window
    {
        public InputBox()
        {
            InitializeComponent();
            Docker.LoadLang(this, "Lang");
            newName.Text = Docker.InputStr;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Docker.InputStr = newName.Text;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Docker.InputStr = "";
            this.Close();
        }
    }
}
