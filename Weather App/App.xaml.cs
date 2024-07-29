using Microsoft.Maui.Controls;

namespace WeatherApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Set the main page of the application to a ContentPage
            MainPage = new MainPage();
        }
    }
}
