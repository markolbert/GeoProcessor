using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.GeoProcessor
{
    public interface IGeoConfig
    {
        public ProcessorType ProcessorType { get; set; }
        public ProcessorInfo? ProcessorInfo { get; }
    }
}
