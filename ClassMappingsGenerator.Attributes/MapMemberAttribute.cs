using System.Reflection;

namespace vsc.ClassMappingsGenerator.Attributes
{
    /// <summary>
    /// Specifies the direction of mapping for a member between a local class and a mapped class.
    /// </summary>
    [Flags]
    public enum MapMemberAction
    {
        /// <summary>
        /// Copy value from mapped class to local class (getter in mapped class, setter in local class).
        /// </summary>
        CopyToSource = 1 << 2,

        /// <summary>
        /// Copy value from local class to mapped class (getter in local class, setter in mapped class).
        /// </summary>
        CopyToTarget = 1 << 3,
    }

    /// <summary>
    /// Attribute to specify mapping information for a member in a local class to a member in a mapped class.
    /// Only one side (the local class) should have this attribute.
    /// <para>
    /// <b>Note:</b> Only public instance members are supported at runtime.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Delegate | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class MapMemberAttribute : Attribute
    {
        /// <summary>
        /// The mapped class type (the remote class).
        /// </summary>
        public Type MappedClassType { get; }

        /// <summary>
        /// The member in the mapped class to get value from (when copying from mapped class to local class).
        /// </summary>
        public MemberInfo? MappedMemberGetter { get; }

        /// <summary>
        /// The member in the mapped class to set value to (when copying from local class to mapped class).
        /// </summary>
        public MemberInfo? MappedMemberSetter { get; }

        /// <summary>
        /// Optional region name for grouping mappings.
        /// </summary>
        public string? RegionName { get; }

        /// <summary>
        /// Optional comment for documentation.
        /// </summary>
        public string? Comment { get; }

        /// <summary>
        /// The mapping direction.
        /// </summary>
        public MapMemberAction MapAction { get; }

        /// <summary>
        /// True if mapping is from mapped class to local class (CopyToSource).
        /// </summary>
        public bool CopyToSource => MapAction.HasFlag(MapMemberAction.CopyToSource);

        /// <summary>
        /// True if mapping is from local class to mapped class (CopyToTarget).
        /// </summary>
        public bool CopyToTarget => MapAction.HasFlag(MapMemberAction.CopyToTarget);

        /// <summary>
        /// Creates a mapping between this member and a member in the mapped class.
        /// </summary>
        /// <param name="mappedClassType">The mapped class type.</param>
        /// <param name="mappedMemberName">The name of the member in the mapped class to map to/from. Must be a public field, property, or method.</param>
        /// <param name="action">The mapping direction.</param>
        /// <param name="comment">Optional comment for documentation.</param>
        /// <param name="regions">Optional region grouping.</param>
        /// <exception cref="ArgumentException">Thrown if the mapped member does not exist in the mapped class or is not public.</exception>
        protected MapMemberAttribute(
            Type mappedClassType,
            string mappedMemberName,
            MapMemberAction action = MapMemberAction.CopyToSource | MapMemberAction.CopyToTarget,
            string? comment = null,
            string[]? regions = null)
        {
            RegionName = regions is null ? null : string.Join(" > ", regions.Where(r => !string.IsNullOrEmpty(r)));
            MappedClassType = mappedClassType ?? throw new ArgumentNullException(nameof(mappedClassType));
            MapAction = action;
            Comment = comment;

            var mappedMembers = mappedClassType.GetMember(mappedMemberName, BindingFlags.Instance | BindingFlags.Public);
            if (mappedMembers.Length == 0)
                throw new ArgumentException($"Member '{mappedMemberName}' not found as a public instance member in type '{mappedClassType.FullName}'. Only public members are supported at runtime.");

            if (CopyToSource)
                MappedMemberGetter = mappedMembers.Single();
            if (CopyToTarget)
                MappedMemberSetter = mappedMembers.Single();
        }

        /// <summary>
        /// Creates a mapping between this member and different getter/setter members in the mapped class.
        /// </summary>
        /// <param name="mappedClassType">The mapped class type.</param>
        /// <param name="getterMemberName">The name of the member in the mapped class to get value from. Must be a public field, property, or method.</param>
        /// <param name="setterMemberName">The name of the member in the mapped class to set value to. Must be a public field, property, or method.</param>
        /// <param name="comment">Optional comment for documentation.</param>
        /// <exception cref="ArgumentException">Thrown if the mapped members do not exist in the mapped class or are not public.</exception>
        protected MapMemberAttribute(
            Type mappedClassType,
            string getterMemberName,
            string setterMemberName,
            string? comment = null)
        {
            MappedClassType = mappedClassType ?? throw new ArgumentNullException(nameof(mappedClassType));
            MapAction = MapMemberAction.CopyToSource | MapMemberAction.CopyToTarget;
            Comment = comment;

            var getterMembers = mappedClassType.GetMember(getterMemberName, BindingFlags.Instance | BindingFlags.Public);
            var setterMembers = mappedClassType.GetMember(setterMemberName, BindingFlags.Instance | BindingFlags.Public);

            if (getterMembers.Length == 0)
                throw new ArgumentException($"Getter member '{getterMemberName}' not found as a public instance member in type '{mappedClassType.FullName}'. Only public members are supported at runtime.");
            if (setterMembers.Length == 0)
                throw new ArgumentException($"Setter member '{setterMemberName}' not found as a public instance member in type '{mappedClassType.FullName}'. Only public members are supported at runtime.");

            MappedMemberGetter = getterMembers.Single();
            MappedMemberSetter = setterMembers.Single();
        }
    }

    /// <summary>
    /// Generic attribute for mapping a member to a member in a mapped class.
    /// </summary>
    /// <typeparam name="TClassType">The mapped class type.</typeparam>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Delegate | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class MapMemberAttribute<TClassType> : MapMemberAttribute
    {
        /// <summary>
        /// Creates a mapping between this member and a member in the mapped class.
        /// </summary>
        /// <param name="mappedMemberName">The name of the member in the mapped class to map to/from. Must be a public field, property, or method.</param>
        /// <param name="action">The mapping direction.</param>
        /// <param name="comment">Optional comment for documentation.</param>
        /// <param name="regions">Optional region grouping.</param>
        public MapMemberAttribute(
            string mappedMemberName,
            MapMemberAction action = MapMemberAction.CopyToSource | MapMemberAction.CopyToTarget,
            string? comment = null,
            string[]? regions = null)
            : base(typeof(TClassType), mappedMemberName, action, comment, regions)
        {
        }

        /// <summary>
        /// Creates a mapping between this member and different getter/setter members in the mapped class.
        /// </summary>
        /// <param name="getterMemberName">The name of the member in the mapped class to get value from. Must be a public field, property, or method.</param>
        /// <param name="setterMemberName">The name of the member in the mapped class to set value to. Must be a public field, property, or method.</param>
        /// <param name="comment">Optional comment for documentation.</param>
        public MapMemberAttribute(
            string getterMemberName,
            string setterMemberName,
            string? comment = null)
            : base(typeof(TClassType), getterMemberName, setterMemberName, comment)
        {
        }
    }

    /// <summary>
    /// Attribute for single direction mapping from mapped class to local class.
    /// </summary>
    /// <typeparam name="TClassType">The mapped class type.</typeparam>
    /// <param name="mappedMemberName">The name of the member in the mapped class to map from. Must be a public field, property, or method.</param>
    /// <param name="comment">Optional comment for documentation.</param>
    /// <param name="regions">Optional region grouping.</param>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Delegate | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class MapFromMemberAttribute<TClassType> : MapMemberAttribute<TClassType>
    {
        /// <summary>
        /// Creates a mapping from a mapped class member to the local class member.
        /// </summary>
        /// <param name="mappedMemberName">The name of the member in the mapped class to map from.</param>
        /// <param name="comment">Optional comment for documentation.</param>
        /// <param name="regions">Optional region grouping.</param>
        public MapFromMemberAttribute(
            string mappedMemberName,
            string? comment = null,
            string[]? regions = null)
            : base(mappedMemberName, MapMemberAction.CopyToSource, comment, regions)
        {
        }
    }

    /// <summary>
    /// Attribute for single direction mapping from local class to mapped class.
    /// </summary>
    /// <typeparam name="TClassType">The mapped class type.</typeparam>
    /// <param name="mappedMemberName">The name of the member in the mapped class to map to. Must be a public field, property, or method.</param>
    /// <param name="comment">Optional comment for documentation.</param>
    /// <param name="regions">Optional region grouping.</param>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Delegate | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class MapToMemberAttribute<TClassType> : MapMemberAttribute<TClassType>
    {
        /// <summary>
        /// Creates a mapping from the local class member to a mapped class member.
        /// </summary>
        /// <param name="mappedMemberName">The name of the member in the mapped class to map to.</param>
        /// <param name="comment">Optional comment for documentation.</param>
        /// <param name="regions">Optional region grouping.</param>
        public MapToMemberAttribute(
            string mappedMemberName,
            string? comment = null,
            string[]? regions = null)
            : base(mappedMemberName, MapMemberAction.CopyToTarget, comment, regions)
        {
        }
    }
}
