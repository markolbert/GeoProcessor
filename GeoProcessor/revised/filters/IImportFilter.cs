using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.GeoProcessor;

public interface IImportFilter : IEqualityComparer<IImportFilter>
{
    string FilterName { get; }

    List<ImportedRoute> Filter( List<ImportedRoute> input );
}