using Microsoft.AspNetCore.DataProtection;

namespace J4JSoftware.GeoProcessor
{
    public interface IDataProtection
    {
        IDataProtector Protector { get; }
    }
}