using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace J4JSoftware.KMLProcessor
{
    public class Coordinates : IEnumerable<Coordinate>
    {
        internal List<Coordinate> Points { get; } = new List<Coordinate>();

        public void Add(string text)
        {
            Points.Add(new Coordinate(text, this));
        }

        public void Add( Coordinate point )
        {
            if( Points.Any( p => p == point ) )
                return;

            Points.Add( new Coordinate( point.Latitude, point.Longitude, this ) );
        }

        public int Count => Points.Count;

        public void Clear()
        {
            Points.Clear();
        }

        public Coordinate? this[ int idx ]
        {
            get
            {
                if( idx < 0 || idx > Points.Count - 1 )
                    return null;

                return Points[ idx ];
            }
        }

        public Coordinate? Average
        {
            get
            {
                if( Points.Count == 0 )
                    return null;

                return new Coordinate(
                    Points.Average( p => p.Latitude ),
                    Points.Average( p => p.Longitude ),
                    this
                );
            }
        }

        public IEnumerator<Coordinate> GetEnumerator()
        {
            foreach( var point in Points )
            {
                yield return point;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}
