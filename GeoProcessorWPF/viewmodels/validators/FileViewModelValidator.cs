using System.IO;
using System.Linq;
using FluentValidation;

namespace J4JSoftware.GeoProcessor
{
    public class FileViewModelValidator : AbstractValidator<FileViewModel>
    {
        public FileViewModelValidator()
        {
            RuleFor( x => x.InputPath )
                .Must( File.Exists )
                .WithMessage( ( fvm, x ) => $"File '{x}' does not exist" );

            RuleFor( x => x.SnappingTypes )
                .Must( x => x.Any() )
                .WithMessage( "No snap-to-route processors are defined" );

            RuleFor( x => x.SelectedSnappingType )
                .Must( x => x != ProcessorType.Undefined && x != ProcessorType.Distance )
                .WithMessage( "You must select a snap-to-route engine" );
            
            RuleFor( x => x.OutputPath )
                .NotEmpty()
                .WithMessage( "An output file must be specified" );
        }
    }
}
