using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace CarbonitePersist.Entity
{
    public class ObjectHandler
    {
        public static TEntity<T> ConvertToEntity<T>(T input)
        {
            var output = new TEntity<T>();

            var type = input.GetType();
            if (type.GetProperty("Id") != null)
            {
                output.Id = type.GetProperty("Id").GetValue(input);
            }
            else if (type.GetProperty("id") != null)
            {
                output.Id = type.GetProperty("id").GetValue(input);
            }
            else
            {
                output.Id = Guid.NewGuid();
            }

            output.Entity = input;

            return output;
        }

        public static bool IsPropertyExist(dynamic entity, string name)
        {
            if (entity is ExpandoObject)
                return ((IDictionary<string, object>)entity).ContainsKey(name);

            return entity.GetType().GetProperty(name) != null;
        }
    }
}
