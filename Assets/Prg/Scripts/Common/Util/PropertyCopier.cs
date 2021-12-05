using System.Reflection;

namespace Prg.Scripts.Common.Util
{
    /// <summary>
    /// Utility class to copy properties and fields by name and type between two instances that can be related or unrelated using <c>Reflection</c>.
    /// </summary>
    /// <typeparam name="TSource">The source type</typeparam>
    /// <typeparam name="TTarget">The target type</typeparam>
    public static class PropertyCopier<TSource, TTarget>
        where TSource : class
        where TTarget : class
    {
        public static void CopyProperties(TSource source, TTarget target)
        {
            var parentProperties = source.GetType().GetProperties();
            var childProperties = target.GetType().GetProperties();

            foreach (var parentProperty in parentProperties)
            {
                foreach (var childProperty in childProperties)
                {
                    if (parentProperty.Name == childProperty.Name && parentProperty.PropertyType == childProperty.PropertyType)
                    {
                        childProperty.SetValue(target, parentProperty.GetValue(source));
                        break;
                    }
                }
            }
        }

        public static void CopyFields(TSource source, TTarget target)
        {
            var parentFields = source.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            var childFields = target.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (var parentField in parentFields)
            {
                foreach (var childField in childFields)
                {
                    if (parentField.Name == childField.Name && parentField.FieldType == childField.FieldType)
                    {
                        childField.SetValue(target, parentField.GetValue(source));
                        break;
                    }
                }
            }
        }
    }
}