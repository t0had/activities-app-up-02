using System.Windows;
using System.Windows.Navigation;

namespace ActivitiesApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            NavigationServiceEx.Init(RootFrame);
            NavigationServiceEx.Navigate(new Pages.LoginPage());
        }
    }
}
