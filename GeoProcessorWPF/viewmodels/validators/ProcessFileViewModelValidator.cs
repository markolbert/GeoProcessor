using System.IO;
using System.Linq;
using FluentValidation;

namespace J4JSoftware.GeoProcessor
{
    public class ProcessFileViewModelValidator : AbstractValidator<ProcessFileViewModel>
    {
        public ProcessFileViewModelValidator()
        {
            RuleFor( x => x.SnappingTypes )
                .Must( x => x.Any() )
                .WithMessage( "No snap-to-route processors are defined" );

            RuleFor( x => x.SelectedSnappingType )
                .Must( x => x != ProcessorType.None && x != ProcessorType.Distance )
                .WithMessage( "You must select a snap-to-route engine" );
        }
    }
}
