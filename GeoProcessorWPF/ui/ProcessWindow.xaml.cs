using System.Windows;
using MahApps.Metro.Controls;

namespace J4JSoftware.GeoProcessor
{
    /// <summary>
    /// Interaction logic for ProcessWindow.xaml
    /// </summary>
    public partial class ProcessWindow : MetroWindow
    {
        private readonly IProcessorViewModel _vm;

        public ProcessWindow( IProcessorViewModel vm )
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
