namespace Radar.Aws.CloudFormation.CustomResources.Util
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Reflection;

    public static class DynamicUtil
    {
        public static dynamic ToDynamic(object obj)
        {
            IDictionary<string, object> expando = new ExpandoObject();
            foreach (PropertyInfo property in obj.GetType().GetProperties())
                expando.Add(property.Name, property.GetValue(obj));

            return expando as ExpandoObject;        
        }

        public static T FromDynamic<T>(dynamic obj)
        {
            string serialized = SerializeUtil.Serialize(obj);
            return SerializeUtil.Deserialize<T>(serialized);
        }

        public static bool IsNull(object obj)
        {
            return object.ReferenceEquals(null, obj);
        }
    }
}
