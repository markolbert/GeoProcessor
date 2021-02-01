using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;

namespace J4JSoftware.GeoProcessor
{
    /// <summary>
    /// Interaction logic for ProcessWindow.xaml
    /// </summary>
    public partial class ProcessWindow : MetroWindow
    {
        private IProcessFileViewModel _vm;

        public ProcessWindow( IProcessFileViewModel vm )
        {
            _vm = vm;

            InitializeComponent();
        }

        private void ProcessWindow_OnLoaded( object sender, RoutedEventArgs e )
        {
            _vm.OnWindowLoadedAsync();
        }
    }
}
