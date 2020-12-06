using System.Collections.Generic;
using J4JSoftware.Logging;
#pragma warning disable 8618

namespace J4JSoftware.KMLProcessor
{
    public class LoggingChannelConfig : LogChannels
    {
        public DebugConfig Debug { get; set; }
        public ConsoleConfig Console { get; set; }

        public override IEnumerator<IChannelConfig> GetEnumerator()
        {
            yield return Console;
            yield return Debug;
        }
    }
}