using System.Reflection;
using vsc.ClassMappingsGenerator.Attributes;

namespace vsc.ClassMappingsGenerator.Mappings
{
    public class TypeComparer : IComparer<Type>
    {
        public int Compare(Type? a, Type? b)
        {
            if (a == b) return 0;                   // Same type
            if (a == null) return -1;               // null is less than anything
            if (b == null) return 1;                // anything is greater than null
            if (a.IsAssignableFrom(b)) return -1;   // a is a base type of b
            if (b.IsAssignableFrom(a)) return 1;    // b is a base type of a
            return 0;                               // No direct assignable relationship
        }
    }

    public static class SystemTypeExtensions
    {
        /// <summary>
        /// Determines whether the specified <see cref="Type"/> is nullable.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to check.</param>
        /// <returns>
        /// <see langword="true"/> if the specified <see cref="Type"/> is nullable; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsNullable(this Type type)
        {
            // Check for Nullable<T> (value types like int? are nullable)
            if (Nullable.GetUnderlyingType(type) != null) return true;

            return false; // Assume non-nullable if it's a value type and not Nullable<T>
        }
        /// <summary>
        /// Check if the type is a basic type (primitive, enum, string, float, decimal)
        /// </summary>
        /// <param name="type"></param>
        /// <returns>
        /// <see langword="true"/> if the type is <see cref="Type.IsPrimitive"/>, <see cref="Type.IsEnum"/>, <see cref="string"/>, <see cref="float"/>, <see cref="decimal"/>
        /// <br/>
        /// <see langword="false"/> otherwise
        /// </returns>
        public static bool IsBasicType(this Type? type)
        {
            if (type == null || type.IsGenericType) return false;

            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            return underlyingType.IsPrimitive || underlyingType.IsEnum || underlyingType == typeof(string) || underlyingType == typeof(float) || underlyingType == typeof(decimal);
        }

        /// <summary>
        /// Check if the type implements <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <param name="type"></param>
        /// <returns>
        /// <see langword="true"/> if the type implements <see cref="IEnumerable{T}"/>
        /// <br/>
        /// <see langword="false"/> otherwise
        /// </returns>
        public static bool IsIEnumerable(this Type? type)
        {
            if (type == null) return false;

            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            return underlyingType.IsGenericType && (underlyingType.GetInterface(typeof(IEnumerable<>).Name) != null || underlyingType.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        }

        /// <summary>
        /// Check if the type implements <see cref="ICollection{T}"/>
        /// </summary>
        /// <param name="type"></param>
        /// <returns>
        /// <see langword="true"/> if the type implements <see cref="ICollection{T}"/>
        /// <br/>
        /// <see langword="false"/> otherwise
        /// </returns>
        public static bool IsICollection(this Type? type)
        {
            if (type == null) return false;

            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            return !type.IsBasicType() && underlyingType.IsGenericType && (underlyingType.GetInterface(typeof(ICollection<>).Name) != null || underlyingType.GetGenericTypeDefinition() == typeof(ICollection<>));
        }

        public static bool IsList(this Type? type)
        {
            if (type == null) return false;

            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            return !type.IsBasicType() && underlyingType.IsGenericType && (underlyingType.GetInterface(typeof(List<>).Name) != null || underlyingType.GetGenericTypeDefinition() == typeof(List<>));
        }

        #region MapAttribute
        /// <summary>
        /// Retrieves all custom attributes of type <see cref="MapAttribute{T1, T2, T3}"/> from the specified <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to inspect for custom attributes.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of <see cref="IMapAttribute"/> instances found on the specified <see cref="Type"/>.
        /// </returns>
        public static IEnumerable<MapAttribute> GetMapAttributes(this Type type)
        {
            return type.GetCustomAttributes(typeof(MapAttribute)).Cast<IMapAttribute>();
        }

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
            => types.Where(t => includeBaseType ? baseType.IsAssignableFrom(t) : t != baseType && baseType.IsAssignableFrom(t)).Distinct();

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
            => (includeBaseType ? type == baseType : type != baseType) && baseType.IsAssignableFrom(type);

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
            => types.Where(t => includeBaseType ? parentType.IsBaseTypeOf(t) : t != parentType && parentType.IsBaseTypeOf(t)).Distinct();

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
            => (includeChildType ? type == childType : type != childType) && type.IsAssignableFrom(childType);

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

            var descendants = universe?.Where(t => t.IsSubclassOf(type)) ?? Enumerable.Empty<Type>();
            return descendants;
        }
        /// <summary>
        /// Filters the collection of <see cref="Type"/> to include only root elements (types that do not inherit from any other type in the collection).
        /// </summary>
        /// <param name="types">The collection of <see cref="Type"/> to filter.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of root <see cref="Type"/> instances.
        /// </returns>
        public static IEnumerable<Type> RootElements(this IEnumerable<Type> types)
            => types.Where(t => t.BaseType == null || !types.Contains(t.BaseType)).Distinct();

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
            => types.RootElements().OrderBy(r => r.Name).ToDictionary(root => root, root => (IEnumerable<Type>)types.WhereInheritsFrom(root).OrderBy(t => t.Name));

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
            var a = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList().Select(p => p.GetCustomAttributes(false)).ToList();
            var b = a.SelectMany(a => a).Where(a => a.GetType().FullName == typeof(T).FullName).ToList();
            // Get properties with attribute T 
            var _properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                    .SelectMany(
                                        p => p.GetCustomAttributes(false).Where(attr => attr.GetType().FullName == typeof(T).FullName)
                                        , (m, p) => (m, p)
                                    );
            var properties = _properties.Select(i => (member: (MemberInfo)i.m, attribute: (T)i.p));

            // Get fields with with attribute T 
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                                    .SelectMany(
                                        f => f.GetCustomAttributes(false).Where(attr => attr.GetType().FullName == typeof(T).FullName)
                                        , (m, f) => (m, f)
                                    )
                                    .Select(i => (member: (MemberInfo)i.m, attribute: (T)i.f));

            return properties.Concat(fields);
        }

        #endregion Get Root Elements
    }
