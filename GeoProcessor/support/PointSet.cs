using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.GeoProcessor
{
    public class PointSet
    {
        public LinkedList<Coordinate> Points { get; set; } = new LinkedList<Coordinate>();
        public string RouteName { get; set; } = "Unnamed Route";
    }
}
