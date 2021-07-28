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

using System.IO;
using System.Linq;
using FluentValidation;

namespace J4JSoftware.GeoProcessor
{
    public class MainVMValidator : AbstractValidator<MainVM>
    {
        public MainVMValidator()
        {
            RuleFor( x => x.InputPath )
                .Must( File.Exists )
                .WithMessage( ( fvm, x ) => $"Input file '{x}' does not exist" );

            RuleFor( x => x.OutputPath )
                .NotEmpty()
                .WithMessage( "An output file must be specified" );

            // x?.Any() ?? false is required because at design time the processors are not defined
            RuleFor( x => x.SnapToRouteProcessors )
                .Must( x => x?.Any() ?? false )
                .WithMessage( "No snap-to-route processors are defined" );

            RuleFor( x => x.SelectedSnapToRouteProcessor )
                .Must( x => x != ProcessorType.None && x != ProcessorType.Distance )
                .WithMessage( "You must select a snap-to-route processor" );
        }
    }
}