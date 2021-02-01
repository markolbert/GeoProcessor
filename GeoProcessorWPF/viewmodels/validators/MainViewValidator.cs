using System.IO;
using System.Linq;
using FluentValidation;

namespace J4JSoftware.GeoProcessor
{
    public class MainViewValidator : AbstractValidator<IMainViewModel>
    {
        public MainViewValidator()
        {
            RuleFor( x => x.InputPath )
                .Must( File.Exists )
                .WithMessage( ( fvm, x ) => $"Input file '{x}' does not exist" );

            RuleFor( x => x.OutputPath )
                .NotEmpty()
                .WithMessage( "An output file must be specified" );

            RuleFor( x => x.SnapToRouteProcessors )
                .Must( x => x.Any() )
                .WithMessage( "No snap-to-route processors are defined" );

            RuleFor( x => x.SelectedSnapToRouteProcessor )
                .Must( x => x != ProcessorType.None && x != ProcessorType.Distance )
                .WithMessage( "You must select a snap-to-route processor" );
        }
    }
}
