using System;

namespace J4JSoftware.KMLProcessor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ImporterAttribute : Attribute
    {
        public ImporterAttribute( ImportType type )
        {
            Type = type;
        }

        public ImportType Type { get; }
    }
}