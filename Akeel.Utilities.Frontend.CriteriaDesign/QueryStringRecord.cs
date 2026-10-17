
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Akeel.Utilities.Frontend.CriteriaDesign
{
    public record QueryStringRecord(string Key, string Value);

    public class QueryStringBuilder
    {
        private List<QueryStringRecord> _queryStrings;

        QueryStringBuilder()
        {
            _queryStrings = new List<QueryStringRecord>();
        }
        public static QueryStringBuilder Instance => new();

        public QueryStringBuilder Add(params QueryStringRecord[] queryStrings)
        {
            if (queryStrings is null || queryStrings.Length == 0)
                return this;

            queryStrings.ToList().ForEach(row =>
            {
                if (_queryStrings.Any(c => c.Key.ToLower() == row.Key.ToLower()) == false)
                    _queryStrings.Add(row);
            });

            return this;
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
                        new ($"{prefixKey}{p.Name}", p.GetValue(obj, null).ToString())
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

        public QueryStringBuilder AddComplexObject<T>(T obj, bool removeNullOrEmpty = true) where T : class
        {
            var objType = obj.GetType();

            var properties = objType.GetProperties().Where(p => !Attribute.IsDefined(p, typeof(NotMappedAttribute))).AsEnumerable().ToList();
            if (properties.Any() == false)
                return this;
            if (removeNullOrEmpty)
                properties = properties.Where(p => p.GetValue(obj, null) != null).ToList();

            properties.ToList().ForEach(p =>
            {
                if (p is null)
                    return;

                if (IsSimple(p.PropertyType))
                {
                    _queryStrings.Add(new QueryStringRecord(p.Name, p.GetValue(obj, null).ToString()));
                    return;
                }

                var isEnumerableType = p.PropertyType.GetInterfaces().Any(q => q.IsGenericType
                                                                               && q.GetGenericTypeDefinition() == typeof(IEnumerable<>));

                if (isEnumerableType)
                {

                    var collection = p.GetValue(obj, null) as ICollection;
                    foreach (object o in collection)
                    {
                        _queryStrings.Add(new QueryStringRecord(p.Name, HttpUtility.UrlEncode(o.ToString())));
                    }
                    return;

                }



                if (p.PropertyType.IsClass)
                {
                    BuildSecondBranch(p.Name, p.GetValue(obj, null), removeNullOrEmpty);
                }



            });


            return this;
        }

        public QueryStringBuilder AddComplexObjectRecursive<T>(T obj, bool removeNullOrEmpty = true) where T : class
        {
            _queryStrings.AddRange(GenerateWithRecursive(obj, removeNullOrEmpty, string.Empty));
            return this;
        }

        private void BuildSecondBranch<T>(string key, T obj, bool removeNullOrEmpty = true)
        {
            var objType = obj.GetType();

            var properties = objType.GetProperties().Where(p => !Attribute.IsDefined(p, typeof(NotMappedAttribute))).AsEnumerable().ToList();
            if (properties.Any() == false)
                return;
            if (removeNullOrEmpty)
                properties = properties.Where(p => p.GetValue(obj, null) != null).ToList();



            properties.ToList().ForEach(p =>
            {
                if (p is null)
                    return;

                if (IsSimple(p.PropertyType))
                {
                    _queryStrings.Add(new QueryStringRecord($"{key}.{p.Name}", p.GetValue(obj, null).ToString()));
                    return;
                }



                var isEnumerableType = p.PropertyType.GetInterfaces().Any(q => q.IsGenericType
                                                       && q.GetGenericTypeDefinition() == typeof(IEnumerable<>));

                if (isEnumerableType)
                {

                    var collection = p.GetValue(obj, null) as ICollection;
                    foreach (object o in collection)
                    {
                        _queryStrings.Add(new QueryStringRecord($"{key}.{p.Name}", HttpUtility.UrlEncode(o.ToString())));
                    }

                }


            });
        }
        public string Build()
        {
            IEnumerable<string> parts = _queryStrings.Select(c =>
                $"{HttpUtility.UrlEncode(c.Key)}={HttpUtility.UrlEncode(c.Value)}");


            return "?" + string.Join("&", parts);
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




        private List<QueryStringRecord> ConvertProperty<T>(PropertyInfo p, T obj, string prefixKey = "", bool removeNullOrEmpty = true)
        {

            if (p is null)
                return new List<QueryStringRecord>();

            if (string.IsNullOrEmpty(prefixKey) == false)
                prefixKey = $"{prefixKey}.";

            if (IsSimple(p.PropertyType))
            {
                return new List<QueryStringRecord>()
                {
                    new ($"{prefixKey}{p.Name}", p.GetValue(obj, null).ToString())
                };
            }

            var isEnumerableType = p.PropertyType.GetInterfaces().Any(q => q.IsGenericType
                                                                           && q.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (isEnumerableType)
            {

                var collection = p.GetValue(obj, null) as ICollection;
                List<QueryStringRecord> result = new();
                foreach (object o in collection)
                {
                    result.Add(new QueryStringRecord($"{prefixKey}{p.Name}", HttpUtility.UrlEncode(o.ToString())));
                }
                return result;

            }



            if (p.PropertyType.IsClass)
            {
                return ConvertProperty(p, p.GetValue(obj, null), $"{prefixKey}{p.Name}");
            }

            return new List<QueryStringRecord>();
        }

    }

}
