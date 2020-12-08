using System.ComponentModel;

namespace J4JSoftware.KMLProcessor
{
    public class Distance
    {
        public Distance( UnitTypes originalUnit, double originalValue )
        {
            OriginalUnit = originalUnit;
            OriginalValue = originalValue;
        }

        public UnitTypes OriginalUnit { get; }
        public double OriginalValue { get; }

        public double GetValue( UnitTypes outUnit )
        {
            if( outUnit == OriginalUnit )
                return OriginalValue;

            return OriginalUnit switch
            {
                UnitTypes.Feet => outUnit switch
                {
                    UnitTypes.Miles => OriginalValue/ 5280,
                    UnitTypes.Meters => OriginalValue * 0.3048,
                    UnitTypes.Kilometers => OriginalValue * 0.0003048,
                    _ => throw new InvalidEnumArgumentException( $"Unsupported unit type '{OriginalUnit}'" )
                },
                UnitTypes.Kilometers => outUnit switch
                {
                    UnitTypes.Miles => OriginalValue * 0.62137119223733,
                    UnitTypes.Meters => OriginalValue * 1000,
                    UnitTypes.Feet => OriginalValue * 3280.8398950131,
                    _ => throw new InvalidEnumArgumentException($"Unsupported unit type '{OriginalUnit}'")
                },
                UnitTypes.Meters => outUnit switch
                {
                    UnitTypes.Miles => OriginalValue * 0.00062137119223733,
                    UnitTypes.Kilometers => OriginalValue / 1000,
                    UnitTypes.Feet => OriginalValue * 3.2808398950131,
                    _ => throw new InvalidEnumArgumentException($"Unsupported unit type '{OriginalUnit}'")
                },
                UnitTypes.Miles => outUnit switch
                {
                    UnitTypes.Meters => OriginalValue * 1609.344,
                    UnitTypes.Kilometers => OriginalValue * 1.609344,
                    UnitTypes.Feet => OriginalValue * 5280,
                    _ => throw new InvalidEnumArgumentException($"Unsupported unit type '{OriginalUnit}'")
                },
                _ => throw new InvalidEnumArgumentException( $"Unsupported unit type '{OriginalUnit}'" )
            };
        }
    }
}