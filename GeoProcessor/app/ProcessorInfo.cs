using System.Text.Json.Serialization;

namespace J4JSoftware.GeoProcessor
{
    public class ProcessorInfo
    {
        public const int DefaultMaxPointsPerRequest = 100;

        private int _maxDistMult = 3;
        private int _maxPoints = DefaultMaxPointsPerRequest;
        private Distance? _maxSep;

        public bool RequiresKey { get; set; }
        public bool HasPointsLimit { get; set; }
        public bool SupportsSnapping { get; set; }

        public int MaxDistanceMultiplier
        {
            get => _maxDistMult;
            set => _maxDistMult = value <= 0 ? 3 : value;
        }

        public int MaxPointsPerRequest
        {
            get => HasPointsLimit ? _maxPoints : int.MaxValue;

            set =>
                _maxPoints = value switch
                {
                    < 0 => int.MaxValue,
                    0 => DefaultMaxPointsPerRequest,
                    _ => value
                };
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