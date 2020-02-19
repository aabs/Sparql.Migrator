using System;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query;

namespace Sparql.Migrator
{
    public static class SparqlHelpers
    {
        public static T GetLiteralValue<T>(this INode node)
        {
            if (node.NodeType == NodeType.Literal)
            {
                return (T)Convert.ChangeType(((ILiteralNode)node).Value, typeof(T));
            }
            throw new RdfException("node type wasn't literal value");
        }

        public static object GetLiteralValue(this INode node, Type t)
        {
            if (node.NodeType == NodeType.Literal)
            {
                return Convert.ChangeType(((ILiteralNode)node).Value, t);
            }
            throw new RdfException("node type wasn't literal value");
        }

        public static object GetNodeValue(Type propertyType, INode node)
        {
            var vn = node.AsValuedNode();
            if (propertyType == typeof(bool))
            {
                return vn.AsBoolean();
            }

            if (propertyType == typeof(DateTime))
            {
                return vn.AsDateTime();
            }

            if (propertyType == typeof(DateTimeOffset))
            {
                return vn.AsDateTimeOffset();
            }

            if (propertyType == typeof(decimal))
            {
                return vn.AsDecimal();
            }

            if (propertyType == typeof(double))
            {
                return vn.AsDouble();
            }

            if (propertyType == typeof(float))
            {
                return vn.AsFloat();
            }

            if (propertyType == typeof(int))
            {
                return vn.AsInteger();
            }

            if (propertyType == typeof(string))
            {
                return vn.AsString();
            }

            if (propertyType == typeof(TimeSpan))
            {
                return vn.AsTimeSpan();
            }

            throw new ApplicationException($"Unable to import results of type: {propertyType.FullName}");
        }

        public static T GetFieldValue<T>(this SparqlResult r, string fieldName, Func<IValuedNode, T> xform, T fallbackValue = default(T))
        {
            if (r == null || string.IsNullOrWhiteSpace(fieldName))
            {
                return fallbackValue;
            }

            if (r.HasValue(fieldName) && r[fieldName] != null)
            {
                return xform(r[fieldName].AsValuedNode());
            }

            return fallbackValue;
        }

        public static T AsEnum<T>(this string v) where T : struct => Enum.Parse<T>(v, false);
    }
}