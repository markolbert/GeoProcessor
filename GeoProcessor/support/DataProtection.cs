using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;

namespace J4JSoftware.GeoProcessor
{
    public class DataProtection : IDataProtection
    {
        public DataProtection( IDataProtectionProvider provider )
        {
            Protector = provider.CreateProtector( "J4JSoftware.GeoProcessor.DataProtection" );
        }

        public IDataProtector Protector { get; }
    }
}
