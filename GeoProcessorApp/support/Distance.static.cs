using System;
using J4JSoftware.Logging;

namespace J4JSoftware.GeoProcessor
{
    public partial class Distance
    {
        public static bool TryParse(string text, out Distance? result, IJ4JLogger? logger = null)
        {
            result = null;

            var parts = text.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
            {
                logger?.Error<int, string>("Found {0} tokens when parsing '{1}' instead of 2", parts.Length, text);
                return false;
            }

            if (!double.TryParse(parts[0], out var distValue))
            {
                logger?.Error<string>("Could not parse '{0}' as a double", parts[0]);
                return false;
            }

            if (!Enum.TryParse(typeof(UnitTypes), parts[1], true, out var unitType))
            {
                logger?.Error<string, Type>("Could not parse '{0}' as a {1}", parts[1], typeof(UnitTypes));
                return false;
            }

            result = new Distance((UnitTypes)unitType!, distValue);

            return true;
        }
    }
}