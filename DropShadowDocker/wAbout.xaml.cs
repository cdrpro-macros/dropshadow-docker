using System.Globalization;
using System.Windows;
using System.Windows.Input;

namespace DropShadow
{
    public partial class wAbout : Window
    {
        public wAbout()
        {
            InitializeComponent();

            sName.Text = Docker.MName;
            sInfo.Text = "Version: " + Docker.MVer + "\n" +
                "Release date: " + Docker.MDate + "\n" +
                "Copyright © Sanich, 2019";
            sWeb.Text = Docker.MWebSite;
            sEmail.Text = "e-mail: " + Docker.MEmail;
        }

        private void cmClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void sWeb_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(Docker.MWebSite);
            this.Close();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                MessageBox.Show(
                    "Id of current language: " + Docker.DApp.UILanguage.GetHashCode().ToString(CultureInfo.InvariantCulture),
                    Docker.MName,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }

    }
}
