using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Web;

namespace FluentQueryStringBuilder.Net
{
    public class QueryStringBuilder
    {
        private readonly List<QueryStringRecord> _queryStrings;

        QueryStringBuilder()
        {
            _queryStrings = new List<QueryStringRecord>();
        }
        public static QueryStringBuilder Instance => new QueryStringBuilder();

        public QueryStringBuilder Add(string key, string value)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
                return this;

            if (_queryStrings.Any(r => r.Key.ToLower() == key.ToLower()))
                return this;

            _queryStrings.Add(new QueryStringRecord(key, value));

            return this;
        }

        public QueryStringBuilder AddComplexObject<T>(T obj, bool removeNullOrEmpty = true) where T : class
        {
            _queryStrings.AddRange(GenerateWithRecursive(obj, removeNullOrEmpty, string.Empty));
            return this;
        }
        public string Build()
        {
            IEnumerable<string> parts = _queryStrings.Select(c =>
                $"{HttpUtility.UrlEncode(c.Key)}={HttpUtility.UrlEncode(c.Value)}");


            return "?" + string.Join("&", parts);
        }
        List<QueryStringRecord> GenerateWithRecursive<T>(T obj, bool removeNullOrEmpty = true, string prefixKey = "") where T : class
        {
            if (obj is null)
                return new List<QueryStringRecord>();

            var objType = obj.GetType();

            var properties = objType.GetProperties().Where(p => !Attribute.IsDefined(p, typeof(NotMappedAttribute))).AsEnumerable().ToList();
            if (properties.Any() == false)
                return new List<QueryStringRecord>();
            if (removeNullOrEmpty)
                properties = properties.Where(p => p.GetValue(obj, null) != null).ToList();

            if (properties.Any() == false)
                return new List<QueryStringRecord>();
            if (string.IsNullOrEmpty(prefixKey) == false)
                prefixKey = $"{prefixKey}.";

            var rows = properties.ToList().SelectMany(p =>
            {
                if (p is null)
                    return new List<QueryStringRecord>();

                if (IsSimple(p.PropertyType))
                {
                    return new List<QueryStringRecord>()
                    {
                        new QueryStringRecord($"{prefixKey}{p.Name}", p.GetValue(obj, null).ToString())
                    };

                }

                var isEnumerableType = p.PropertyType.GetInterfaces().Any(q => q.IsGenericType
                                                                               && q.GetGenericTypeDefinition() == typeof(IEnumerable<>));

                if (isEnumerableType)
                {
                    var results = new List<QueryStringRecord>();
                    var collection = p.GetValue(obj, null) as ICollection;
                    foreach (object o in collection)
                    {
                        results.Add(new QueryStringRecord($"{prefixKey}{p.Name}", HttpUtility.UrlEncode(o.ToString())));
                    }
                    return results;

                }


                return GenerateWithRecursive(p.GetValue(obj, null), removeNullOrEmpty, $"{prefixKey}{p.Name}");

            }).ToList();


            return rows;
        }
        bool IsSimple(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // nullable type, check if the nested type is simple.
                return IsSimple(typeInfo.GetGenericArguments()[0]);
            }
            return typeInfo.IsPrimitive
                   || typeInfo.IsEnum
                   || type.Equals(typeof(string))
                   || type.Equals(typeof(decimal));
        }
    }
}