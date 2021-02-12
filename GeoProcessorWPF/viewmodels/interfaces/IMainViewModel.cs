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

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace J4JSoftware.GeoProcessor
{
    public interface IMainViewModel : INotifyDataErrorInfo, INotifyPropertyChanged, INotifyPropertyChanging
    {
        // file specification
        ICommand InputFileCommand { get; }
        string InputPath { get; }
        ICommand OutputFileCommand { get; }
        string OutputPath { get; }
        ObservableCollection<ExportType> ExportTypes { get; }
        ExportType SelectedExportType { get; set; }

        // snap-to-route
        ObservableCollection<ProcessorType> SnapToRouteProcessors { get; }
        ProcessorType SelectedSnapToRouteProcessor { get; set; }

        // status
        bool ConfigurationIsValid { get; }
        ObservableCollection<string> Messages { get; }

        // commands
        ICommand EditOptionsCommand { get; }
        ICommand ProcessCommand { get; }
        ICommand AboutCommand { get; }
        ICommand HelpCommand { get; }
    }
}