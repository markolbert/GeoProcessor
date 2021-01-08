using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alba.CsConsoleFormat;
using Alba.CsConsoleFormat.Fluent;
using J4JSoftware.Logging;

namespace J4JSoftware.GeoProcessor
{
    public static class ConsoleExtensions
    {
        private const string Indent = "    ";

        public static T GetEnum<T>(T curValue, T defaultValue, List<T>? values = null, IJ4JLogger? logger = null )
            where T : Enum
        {
            Colors.WriteLine( "Enter ", 
                typeof(T).Name.Yellow(), 
                " (current value is ".White(),
                curValue.ToString().Green(), 
                ") :\n" );

            values ??= Enum.GetValues(typeof(T)).Cast<T>().ToList();

            for (var idx = 0; idx < values.Count; idx++)
            {
                Colors.WriteLine(Indent,
                    (idx + 1).ToString().Green(),
                    " - ",
                    values[idx].ToString());
            }

            Console.Write("\n\nChoice: ");

            var text = Console.ReadLine();

            if (!string.IsNullOrEmpty(text)
                && int.TryParse(text, NumberStyles.Integer, null, out var choice)
                && choice >= 1
                && choice <= values.Count)
                return values[choice - 1];

            return defaultValue;
        }

        public static string GetText(string curValue, string prompt, string? defaultValue = null )
        {
            defaultValue ??= curValue;

            Colors.WriteLine( "Enter ", prompt.Green(), " (current value is '".White(), curValue.Green(), "'): ");
            Console.Write("> ");

            var retVal = Console.ReadLine();

            return string.IsNullOrEmpty(retVal) ? defaultValue : retVal;
        }

        public static string GetText(string curValue, string? defaultValue, params Span[] prompts)
        {
            defaultValue ??= curValue;

            var promptList = prompts.ToList();
            promptList.Insert( 0, "Enter ".White() );
            promptList.Add(" (current value is '".White()  );
            promptList.Add(curValue.Green()  );
            promptList.Add( "') :".White() );

            Colors.WriteLine( promptList );
            Console.Write("> ");

            var retVal = Console.ReadLine();

            return string.IsNullOrEmpty(retVal) ? defaultValue : retVal;
        }
    }
}
