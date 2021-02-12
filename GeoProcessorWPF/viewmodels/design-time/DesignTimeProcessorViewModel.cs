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
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;

// ReSharper disable CollectionNeverUpdated.Local
// ReSharper disable UnassignedGetOnlyAutoProperty
#pragma warning disable 1998
#pragma warning disable 8618
#pragma warning disable 649

namespace J4JSoftware.GeoProcessor
{
    public class DesignTimeProcessorViewModel : ObservableRecipient, IProcessorViewModel
    {
        private string _cmdButtonText = string.Empty;
        private string _phase = string.Empty;
        private int _pointProcessed;

        public DesignTimeProcessorViewModel()
        {
            for( var idx = 0; idx < 5; idx++ ) Messages.Add( $"Message #{idx + 1}" );

            CommandButtonText = "Start";
            Phase = "Ready to begin...";
            PointsProcessed = 55000;
        }

        public string CommandButtonText
        {
            get => _cmdButtonText;
            private set => SetProperty( ref _cmdButtonText, value );
        }

        public ICommand WindowLoaded { get; }

        public ProcessorState ProcessorState { get; }

        public string Phase
        {
            get => _phase;
            set => SetProperty( ref _phase, value );
        }

        public int PointsProcessed
        {
            get => _pointProcessed;
            private set => SetProperty( ref _pointProcessed, value );
        }

        public ObservableCollection<string> Messages { get; } = new();
        public ICommand AbortCommand { get; }
        public ICommand WindowLoadedCommand { get; }
    }
}