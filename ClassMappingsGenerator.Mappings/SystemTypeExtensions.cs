using System.Reflection;
using vsc.ClassMappingsGenerator.Attributes;

namespace vsc.ClassMappingsGenerator.Mappings
{
    public class TypeComparer : IComparer<Type>
    {
        ///<summary>
        /// Compares two <see cref="Type"/> objects based on their assignability.
        /// </summary>
        /// <param name="a">The first <see cref="Type"/> to compare.</param>
        /// <param name="b">The second <see cref="Type"/> to compare.</param>
        /// <returns>
        /// A negative integer if <paramref name="a"/> is assignable from <paramref name="b"/>;
        /// a positive integer if <paramref name="b"/> is assignable from <paramref name="a"/>;
        /// zero if both are the same or neither is assignable from the other.
        /// </summary>
        public int Compare(Type? a, Type? b)
        {
            if (a == b) return 0;
            if (a == null) return -1;
            if (b == null) return 1;
            if (a.IsAssignableFrom(b)) return -1;
            if (b.IsAssignableFrom(a)) return 1;
            return 0;
        }
    }

    public static class SystemTypeExtensions
    {
        /// <summary>
        /// Determines whether the specified <see cref="Type"/> is nullable (i.e., Nullable&lt;T&gt;).
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to check.</param>
        /// <returns>
        /// <see langword="true"/> if the specified <see cref="Type"/> is nullable; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsNullable(this Type type)
        {
            // Check for Nullable<T>.
            return Nullable.GetUnderlyingType(type) != null;
        }

        /// <summary>
        /// Check if the type is a basic type (primitive, enum, string, float, decimal).
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to check.</param>
        /// <returns>
        /// <see langword="true"/> if the type is <see cref="Type.IsPrimitive"/>, <see cref="Type.IsEnum"/>, <see cref="string"/>, <see cref="float"/>, or <see cref="decimal"/>;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public static bool IsBasicType(this Type? type)
        {
            if (type == null || type.IsGenericType) return false;

            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            return underlyingType.IsPrimitive
                || underlyingType.IsEnum
                || underlyingType == typeof(string)
                || underlyingType == typeof(float)
                || underlyingType == typeof(decimal);
        }

        /// <summary>
        /// Check if the type implements <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to check.</param>
        /// <returns>
        /// <see langword="true"/> if the type implements <see cref="IEnumerable{T}"/>;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public static bool IsIEnumerable(this Type? type)
        {
            // Returns true if the type implements IEnumerable<T>.
            if (type == null) return false;

            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            // Check for generic IEnumerable.
            return underlyingType.IsGenericType &&
                   (underlyingType.GetInterface(typeof(IEnumerable<>).Name) != null ||
                    underlyingType.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        }

        /// <summary>
        /// Check if the type implements <see cref="ICollection{T}"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to check.</param>
        /// <returns>
        /// <see langword="true"/> if the type implements <see cref="ICollection{T}"/>;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public static bool IsICollection(this Type? type)
        {
            if (type == null) return false;

            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            // Exclude basic types, check for generic ICollection.
            return !type.IsBasicType() &&
                   underlyingType.IsGenericType &&
                   (underlyingType.GetInterface(typeof(ICollection<>).Name) != null ||
                    underlyingType.GetGenericTypeDefinition() == typeof(ICollection<>));
        }

        /// <summary>
        /// Check if the type implements <see cref="List{T}"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to check.</param>
        /// <returns>
        /// <see langword="true"/> if the type implements <see cref="List{T}"/>;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public static bool IsList(this Type? type)
        {
            if (type == null) return false;

            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            // Exclude basic types, check for generic List.
            return !type.IsBasicType() &&
                   underlyingType.IsGenericType &&
                   (underlyingType.GetInterface(typeof(List<>).Name) != null ||
                    underlyingType.GetGenericTypeDefinition() == typeof(List<>));
        }

        #region MapAttribute

        /// <summary>
        /// Filters the collection of <see cref="Type"/> to include only those that inherit from the specified base <see cref="Type"/>.
        /// </summary>
        /// <param name="types">The collection of <see cref="Type"/> to filter.</param>
        /// <param name="baseType">The base <see cref="Type"/> to check against.</param>
        /// <param name="includeBaseType">Whether to include the base <see cref="Type"/> itself in the results.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of <see cref="Type"/> that inherit from the specified base <see cref="Type"/>.
        /// </returns>
        public static IEnumerable<Type> WhereInheritsFrom(this IEnumerable<Type> types, Type baseType, bool includeBaseType = false)
        {
            return types.Where(t => t.InheritsFrom(baseType, includeBaseType)).Distinct();
        }

        /// <summary>
        /// Determines whether the specified <see cref="Type"/> inherits from the specified base <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to check.</param>
        /// <param name="baseType">The base <see cref="Type"/> to check against.</param>
        /// <param name="includeBaseType">Whether to include the base <see cref="Type"/> itself in the check.</param>
        /// <returns>
        /// <see langword="true"/> if the specified <see cref="Type"/> inherits from the base <see cref="Type"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool InheritsFrom(this Type type, Type baseType, bool includeBaseType = false)
        {
            return (includeBaseType ? type == baseType : type != baseType) && baseType.IsAssignableFrom(type);
        }

        /// <summary>
        /// Filters the collection to include only those that are base types of the specified parent <see cref="Type"/>.
        /// </summary>
        /// <param name="types">The collection of <see cref="Type"/> to filter.</param>
        /// <param name="parentType">The parent <see cref="Type"/> to check against.</param>
        /// <param name="includeBaseType">Whether to include the base <see cref="Type"/> itself in the results.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of <see cref="Type"/> that are base types of the specified parent <see cref="Type"/>.
        /// </returns>
        public static IEnumerable<Type> WhereIsBaseTypeOf(this IEnumerable<Type> types, Type parentType, bool includeBaseType = false)
        {
            return types.Where(t => parentType.IsBaseTypeOf(t,includeBaseType)).Distinct();
        }

        /// <summary>
        /// Determines whether the specified <see cref="Type"/> is a base type of the specified child <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to check.</param>
        /// <param name="childType">The child <see cref="Type"/> to check against.</param>
        /// <param name="includeChildType">Whether to include the child <see cref="Type"/> itself in the check.</param>
        /// <returns>
        /// <see langword="true"/> if the specified <see cref="Type"/> is a base type of the child <see cref="Type"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsBaseTypeOf(this Type type, Type childType, bool includeChildType = false)
        {
            return (includeChildType ? type == childType : type != childType) && type.IsAssignableFrom(childType);
        }

        /// <summary>
        /// Retrieves the base types of the specified <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to retrieve base types for.</param>
        /// <param name="universe">An optional collection of <see cref="Type"/> to limit the results to.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of base <see cref="Type"/> instances.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the specified <see cref="Type"/> is null.</exception>
        public static IEnumerable<Type> BaseTypes(this Type type, IEnumerable<Type>? universe = null)
        {
            // Generated by Copilot: Yields all base types of the given type, optionally filtered by universe.
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var currentType = type.BaseType;
            while (currentType != null)
            {
                if (universe == null || universe.Contains(currentType))
                {
                    yield return currentType;
                }
                currentType = currentType.BaseType;
            }
        }

        /// <summary>
        /// Retrieves the descendant types of the specified <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to retrieve descendant types for.</param>
        /// <param name="universe">An optional collection of <see cref="Type"/> to limit the results to.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of descendant <see cref="Type"/> instances.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the specified <see cref="Type"/> is null.</exception>
        public static IEnumerable<Type> DescendantTypes(this Type type, IEnumerable<Type>? universe = null)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return universe?.Where(t => t.IsSubclassOf(type)) ?? Enumerable.Empty<Type>();
        }

        /// <summary>
        /// Filters the collection of <see cref="Type"/> to include only root elements (types that do not inherit from any other type in the collection).
        /// </summary>
        /// <param name="types">The collection of <see cref="Type"/> to filter.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of root <see cref="Type"/> instances.
        /// </returns>
        public static IEnumerable<Type> RootElements(this IEnumerable<Type> types)
        {
            return types.Where(t => t.BaseType == null || !types.Contains(t.BaseType)).Distinct();
        }

        /// <summary>
        /// Groups the collection of <see cref="Type"/> by their root elements (types that do not inherit from any other type in the collection).
        /// </summary>
        /// <param name="types">The collection of <see cref="Type"/> to group.</param>
        /// <returns>
        /// A <see cref="Dictionary{TKey, TValue}"/> where
        /// <br/><b>keys</b> are root elements (types that do not inherit from any other type in the collection)
        /// <br/>
        /// <b>value</b> is a collection of <see cref="Type"/> that inherit from the key root type.
        /// </returns>
        public static Dictionary<Type, IEnumerable<Type>> GroupByRootElements(this IEnumerable<Type> types)
        {
            return types.RootElements()
                        .OrderBy(r => r.Name)
                        .ToDictionary(
                            root => root,
                            root => (IEnumerable<Type>)types.WhereInheritsFrom(root).OrderBy(t => t.Name)
                        );
        }

        /// <summary>
        /// Retrieves the members of the specified <see cref="Type"/> that have the specified attribute.
        /// </summary>
        /// <typeparam name="T">The type of the attribute to look for.</typeparam>
        /// <param name="type">The <see cref="Type"/> to inspect for members with the specified attribute.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of tuples where each tuple contains a <see cref="MemberInfo"/> and the attribute of type <typeparamref name="T"/>.
        /// </returns>
        public static IEnumerable<(MemberInfo member, T attribute)> GetMembersWithAttribute<T>(this Type type)
        {
            // Generated by Copilot: Get properties with attribute T.
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .SelectMany(
                    prop => prop.GetCustomAttributes(typeof(T), false)
                                .Cast<T>()
                                .Select(attr => (member: (MemberInfo)prop, attribute: attr))
                );

            // Generated by Copilot: Get fields with attribute T.
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .SelectMany(
                    field => field.GetCustomAttributes(typeof(T), false)
                                  .Cast<T>()
                                  .Select(attr => (member: (MemberInfo)field, attribute: attr))
                );

            return properties.Concat(fields);
        }

        #endregion
    }
}