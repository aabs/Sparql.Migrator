using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;

namespace Sparql.Migrator
{
    public static class GraphWalking
    {
        public static Walker Walk(this IGraph g, INode n) => new Walker(g, n);

        /// <summary>
        /// Enumerate walkers for all nodes of a given rdf type
        /// </summary>
        /// <param name="graph">the graph being walked</param>
        /// <param name="nodeType">the type of the nodes being enumerated.</param>
        /// <returns><see cref="IEnumerable{Walker}"/> each of which can allow further walks in different directions</returns>
        /// <remarks>
        /// NB. the <paramref name="nodeType"/> should be in the form of a QName prefixed using a namespace prefix already known to the graph (i.e. such as was provided in the original SPARQL query)
        /// </remarks>
        public static IEnumerable<Walker> WalkAll(this IGraph graph, string nodeType)
        {
            var triples = graph.GetTriplesWithPredicateObject(graph.CreateUriNode("rdf:type"), graph.CreateUriNode(nodeType));
            return triples
                .Select(t => new Walker(graph, t.Subject));
        }

        /// <summary>Extract the value of an RDF data property by walking the predicate</summary>
        /// <typeparam name="T">The type to return the data in</typeparam>
        /// <param name="walker">The walker providing access to the current resource in the graph</param>
        /// <param name="predicate">The name of the predicate/edge to be navigated (as an RDF QName string)</param>
        /// <returns>The value of the target node, extracted and cast to type <see cref="T"/></returns>
        public static T DataProperty<T>(this Walker walker, string predicate)
            => (T)walker.DataProperty(predicate, typeof(T));

        /// <summary>
        /// Walk a predicate from a starting point to a literal node
        /// </summary>
        /// <param name="walker">The walker providing access to the graph</param>
        /// <param name="predicateQName">The name of the predicate/edge to be navigated (as a QName)</param>
        /// <param name="propertyType">The type (as an arg rather than as a generic type param) of the value to extract</param>
        /// <returns>the value of the target node, extracted and cast as a type <paramref name="propertyType"/></returns>
        public static object DataProperty(this Walker walker, string predicateQName, Type propertyType)
        {
            var graph = walker.SourceGraph;
            var predicateNode = graph.GetUriNode(predicateQName);

            // if the predicate comes back null, it means there is no property attached to the resource of this kind (normally :)
            if (predicateNode == null)
            {
                return null;
            }
            return graph.GetTriplesWithSubjectPredicate(walker.CurrentResource, predicateNode)
                .Where(triple => triple.Object.NodeType == NodeType.Literal)
                .Select(triple => triple.Object.GetLiteralValue(propertyType))
                .FirstOrDefault();
        }

        /// <summary>
        /// Walk a predicate to another resource node.
        /// </summary>
        /// <param name="walker">walker context providing navigation capabilities</param>
        /// <param name="predicate">name of the predicate to navigate (as a QName)</param>
        /// <returns>Another walker centred at the new node</returns>
        public static Walker ObjectProperty(this Walker walker, string predicate)
            => walker.Outgoing(predicate).FirstOrDefault();

        /// <summary>
        /// Walks all outgoing predicates
        /// </summary>
        /// <param name="walker">walker context providing navigation capabilities</param>
        /// <param name="predicate">name of the predicate to navigate (as a QName)</param>
        /// <returns>Another walker centred at the new node</returns>
        public static IEnumerable<Walker> Outgoing(this IEnumerable<Walker> wcs, string predicate)
            => wcs.SelectMany(wc => wc.Outgoing(predicate));

        /// <summary>  Retrieves all outgoing triples matching the supplied <see cref="predicate"/>.</summary>
        /// <param name="wc">The walker marking the origin (i.e. Subject) of the outgoing triples.</param>
        /// <param name="predicate">The predicate (as an RDF QName) to match on outgoing predicates</param>
        /// <returns>a possibly empty sequence of <see cref="Walker"/>.</returns>
        public static IEnumerable<Walker> Outgoing(this Walker wc, string predicate)
        {
            var g = wc.SourceGraph;
            var p = g.GetUriNode(predicate);
            if (p == null)
            {
                // if p is null, that means there are no triples in the graph relating to this predicate.
                // in that case, do nothing.
                return Enumerable.Empty<Walker>();
            }
            return g.GetTriplesWithSubjectPredicate(wc.CurrentResource, p)
                .Select(t => new Walker(g, t.Object));
        }

        /// <summary>  Retrieves all incoming triples matching the supplied <see cref="predicate"/>, combined across all starting walkers.</summary>
        /// <param name="wcs">The walkers marking the targets (i.e. Object) of the incoming triples.</param>
        /// <param name="predicate">The predicate (as an RDF QName) to match on incoming predicates</param>
        /// <returns>a possibly empty sequence of <see cref="Walker"/>.</returns>
        public static IEnumerable<Walker> Incoming(this IEnumerable<Walker> wcs, string predicate)
            => wcs.SelectMany(wc => wc.Incoming(predicate));

        /// <summary>  Retrieves all incoming triples matching the supplied <see cref="predicate"/>.</summary>
        /// <param name="wc">The walker marking the target (i.e. Object) of the incoming triples.</param>
        /// <param name="predicate">The predicate (as an RDF QName) to match on incoming predicates</param>
        /// <returns>a possibly empty sequence of <see cref="Walker"/>.</returns>
        public static IEnumerable<Walker> Incoming(this Walker wc, string predicate)
        {
            var g = wc.SourceGraph;
            var p = g.GetUriNode(predicate);
            if (p == null)
            {
                // if p is null, that means there are no triples in the graph relating to this predicate.
                // in that case, do nothing.
                return Enumerable.Empty<Walker>();
            }
            return g.GetTriplesWithPredicateObject(p, wc.CurrentResource)
                .Select(t => new Walker(g, t.Subject));
        }

        /// <summary> Extracts the URI for the resource marked by the Walker <see cref="wc"/>. </summary>
        /// <param name="wc">The walker to extract the URI for.</param>
        /// <returns>A fully qualified URI for the resource identified by the Walker <see cref="wc"/>.</returns>
        public static Uri AsUri(this Walker wc)
            => AsUri(wc.CurrentResource);

        /// <summary>Extracts the URI for the <see cref="INode"/> <see cref="node"/>.</summary>
        /// <param name="node">The Node to extract the URI for.</param>
        /// <returns>A fully qualified URI for the resource identified by the <see cref="INode"/> <see cref="node"/>.</returns>
        /// <exception cref="ApplicationException">if node was blank or literal instead of URI</exception>
        public static Uri AsUri(this INode node)
        {
            if (node.NodeType == NodeType.Uri)
            {
                return ((IUriNode)node).Uri;
            }

            throw new ApplicationException("node type wasn't Uri");
        }
    }
}