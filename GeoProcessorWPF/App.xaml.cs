#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'GeoProcessorWPF' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace J4JSoftware.GeoProcessor
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async void Application_Startup( object sender, StartupEventArgs e )
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async void Application_Exit( object sender, ExitEventArgs e )
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
        }
    }
}