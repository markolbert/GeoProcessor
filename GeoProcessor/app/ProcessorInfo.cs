using System.Text.Json.Serialization;

namespace J4JSoftware.GeoProcessor
{
    public class ProcessorInfo
    {
        private int _maxDistMult = 3;
        private Distance? _maxSep;

        public int MaxDistanceMultiplier
        {
            get => _maxDistMult;
            set => _maxDistMult = value <= 0 ? 3 : value;
        }

        [JsonIgnore]
        public Distance MaxSeparation
        {
            get
            {
                if( !object.Equals( _maxSep, null ) ) 
                    return _maxSep ?? new Distance( UnitTypes.ft, 100 );

                if( Distance.TryParse( MaxSeparationText, out var temp ) )
                    _maxSep = temp;

                return _maxSep ?? new Distance( UnitTypes.ft, 100 );
            }

            set
            {
                _maxSep = value;
                MaxSeparationText = _maxSep.ToString();
            }
        }

        [ JsonPropertyName( "MaxSeparation" ) ]
        private string MaxSeparationText { get; set; } = "100 ft";
    }
}