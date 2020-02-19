using VDS.RDF;

namespace Sparql.Migrator
{
    public class Walker
    {
        /// <summary>
        /// Create a walker for the graph <paramref name="sourceGraph"/> starting out at the node <paramref name="currentResource"/>
        /// </summary>
        /// <param name="sourceGraph">the graph being walked</param>
        /// <param name="currentResource">the starting point for the walk</param>
        public Walker(IGraph sourceGraph, INode currentResource)
        {
            SourceGraph = sourceGraph ?? throw new System.ArgumentNullException(nameof(sourceGraph));
            CurrentResource = currentResource ?? throw new System.ArgumentNullException(nameof(currentResource));
        }

        /// <summary>
        /// The graph being walked
        /// </summary>
        public IGraph SourceGraph { get; }

        /// <summary>
        /// the starting point for the walk
        /// </summary>
        public INode CurrentResource { get; }
    }
}