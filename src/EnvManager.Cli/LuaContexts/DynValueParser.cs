using EnvManager.Cli.Models;
using MoonSharp.Interpreter;
using System.Collections;
using System.Reflection;

namespace EnvManager.Cli.LuaContexts
{
    public static class DynValueParser
    {
        public static Step ParseStep<T>(DynValue dynValue) where T : 
            ITask,
            new()
        {
            if (dynValue.Type != DataType.Table)
                throw new ArgumentException("DynValue must be a table");

            var step = Parse<Step>(dynValue);
            step.Task = Parse<T>(step.Parameters);

            return step;
        }

        public static T Parse<T>(DynValue dynValue) where T : new()
        {
            if (dynValue.Type != DataType.Table)
                throw new ArgumentException("DynValue must be a table");

            var obj = new T();
            ParseTable(dynValue.Table, obj);
            return obj;
        }

        private static void ParseTable(Table table, object obj)
        {
            var objType = obj.GetType();
            foreach (var pair in table.Pairs)
            {
                var key = pair.Key.String.Replace("_", "");
                var value = pair.Value;

                var property = objType.GetProperty(key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (property == null)
                {
                    continue;
                }
                var propertyType = property.PropertyType;

                if (property.PropertyType == typeof(DynValue))
                {
                    property.SetValue(obj, pair.Value);
                }
                else if (pair.Value.Type == DataType.Function)
                {
                    property.SetValue(obj, pair.Value.Function);
                }
                else if (typeof(IEnumerable).IsAssignableFrom(propertyType) && propertyType != typeof(string))
                {
                    var elementType = propertyType.GetGenericArguments().First();
                    var type = typeof(List<>).MakeGenericType(elementType);
                    var values = value.ToObject(type);
                    property.SetValue(obj, values);
                }
                else if (propertyType.IsClass && propertyType != typeof(string))
                {
                    var nestedObj = Activator.CreateInstance(propertyType);
                    if (value.Type == DataType.Table)
                    {
                        ParseTable(value.Table, nestedObj);
                        property.SetValue(obj, nestedObj);
                    }
                }
                else if (propertyType.IsEnum)
                {
                    if (value.Type == DataType.String)
                        property.SetValue(obj, Enum.Parse(propertyType, value.String, true));
                    else
                        property.SetValue(obj, value.ToObject());
                }
                else
                {
                    var convertedValue = Convert.ChangeType(value.ToObject(), propertyType);
                    property.SetValue(obj, convertedValue);
                }
            }
        }
    }
}
