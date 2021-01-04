using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Crypto.Tls;

namespace J4JSoftware.KMLProcessor
{
    public class RouteProcessor : IRouteProcessor
    {
        protected RouteProcessor(
            AppConfig config, 
            IJ4JLogger logger )
        {
            Configuration = config.Processors?.Where( kvp => kvp.Key == config.ProcessorType )
                                .Select( kvp => kvp.Value )
                                .FirstOrDefault()
                            ?? new ProcessorInfo();

            APIKey = config.APIKeys?.Where( kvp => kvp.Key == config.ProcessorType )
                         .Select( k => k.Value )
                         .FirstOrDefault()
                         ?.Value
                     ?? string.Empty;

            Processor = config.ProcessorType;

            Logger = logger;
            Logger.SetLoggedType( GetType() );
        }

        protected IJ4JLogger Logger { get; }

        protected ProcessorInfo Configuration { get; }
        protected string APIKey { get; }
        protected ProcessorType Processor { get; }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected virtual async Task<List<Coordinate>?> ExecuteRequestAsync(
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            List<Coordinate> coordinates,
            CancellationToken cancellationToken)
        {
            return null;
        }

        protected virtual void UpdateOutputList( List<Coordinate> snappedPts, LinkedList<Coordinate> linkedList )
        {
            var prevNode = linkedList.Count == 0 ? null : linkedList.Last;

            foreach( var snappedPt in snappedPts )
                prevNode = prevNode == null
                    ? linkedList.AddFirst( snappedPt )
                    : linkedList.AddAfter( prevNode, snappedPt );
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public virtual async Task<LinkedList<Coordinate>?> ProcessAsync( LinkedList<Coordinate> nodes, CancellationToken cancellationToken )
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            throw new NotImplementedException();
        }
    }
}