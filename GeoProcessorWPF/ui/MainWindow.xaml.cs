using System.Windows;
using MahApps.Metro.Controls;

namespace J4JSoftware.GeoProcessor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly IMainViewModel _vm;

        public MainWindow( IMainViewModel vm )
        {
            _vm = vm;

            InitializeComponent();
        }

        private void Hyperlink_OnClick( object sender, RoutedEventArgs e )
        {
            _vm.OpenHelp();
        }
    }
}
