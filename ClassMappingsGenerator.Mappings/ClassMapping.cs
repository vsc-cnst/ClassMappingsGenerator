using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using vsc.ClassMappingsGenerator.Attributes;

namespace vsc.ClassMappingsGenerator.Mappings
{
    /// <summary>
    /// Represents a mapping between two classes for generating extension methods to copy data between them.
    /// </summary>
    public sealed class ClassMapping : IEquatable<ClassMapping>, IEqualityComparer<ClassMapping>
    {
        /// <summary>
        /// Gets the source type in the mapping.
        /// </summary>
        public Type SourceType { get; }

        /// <summary>
        /// Gets the target type in the mapping.
        /// </summary>
        public Type TargetType { get; }

        /// <summary>
        /// Gets the namespace for the generated code.
        /// </summary>
        public string? OutputNamespaceName { get; }

        /// <summary>
        /// Gets the static class name for the generated extension methods.
        /// </summary>
        public string? OutputStaticClassName { get; }

        /// <summary>
        /// Gets the region name for the generated code.
        /// </summary>
        public string? OutputRegionName { get; }

        /// <summary>
        /// Gets the type of the argument passed to the CopyTo method, if any.
        /// </summary>
        public Type? CopyToTargetArgType { get; }

        /// <summary>
        /// Gets the list of member mappings between source and target classes.
        /// </summary>
        public IReadOnlyList<MemberMapping> MappedMembers { get; }

        private readonly int _hashCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassMapping"/> class.
        /// </summary>
        /// <param name="sourceType">The source type in the mapping.</param>
        /// <param name="targetType">The target type in the mapping.</param>
        /// <param name="outputNamespaceName">The namespace for the generated code.</param>
        /// <param name="outputStaticClassName">The static class name for the generated extension methods.</param>
        /// <param name="outputRegionName">The region name for the generated code.</param>
        /// <param name="copyToTargetArgType">The type of the argument passed to the CopyTo method, if any.</param>
        /// <param name="mappedMembers">The list of member mappings between source and target classes.</param>
        public ClassMapping(
            Type sourceType,
            Type targetType,
            string? outputNamespaceName = null,
            string? outputStaticClassName = null,
            string? outputRegionName = null,
            Type? copyToTargetArgType = null,
            IReadOnlyList<MemberMapping>? mappedMembers = null)
        {
            SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
            TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
            OutputNamespaceName = outputNamespaceName;
            OutputStaticClassName = outputStaticClassName;
            OutputRegionName = outputRegionName;
            CopyToTargetArgType = copyToTargetArgType;
            MappedMembers = mappedMembers ?? Array.Empty<MemberMapping>();

            _hashCode = CalculateHashCode();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassMapping"/> class from a <see cref="MapAttribute"/>.
        /// </summary>
        /// <param name="sourceType">The source type in the mapping.</param>
        /// <param name="attr">The <see cref="MapAttribute"/> containing mapping metadata.</param>
        public ClassMapping(Type sourceType, MapAttribute attr)
            : this(
                sourceType,
                attr?.MappedClassType ?? throw new ArgumentNullException(nameof(attr)),
                attr.OutputNamespaceName,
                attr.OutputStaticClassName,
                attr.OutputRegionName,
                attr.ToMappedClassArgsType)
        {
        }

        /// <summary>
        /// Creates mappings from a collection of types with <see cref="MapAttribute"/>.
        /// </summary>
        /// <param name="types">The types to scan for <see cref="MapAttribute"/>.</param>
        /// <returns>A collection of <see cref="ClassMapping"/> instances.</returns>
        public static IEnumerable<ClassMapping> CreateMappings(IEnumerable<Type> types)
        {
            if (types == null) throw new ArgumentNullException(nameof(types));
            return types
                .SelectMany(type =>
                    type.GetCustomAttributes(typeof(MapAttribute), inherit: false)
                        .OfType<MapAttribute>()
                        .Select(attr => new ClassMapping(type, attr)));
        }

        /// <summary>
        /// Creates member mappings for the given class mapping by matching public properties with the same name and type,
        /// and by considering explicit MapMemberAttribute mappings on both source and target classes.
        /// </summary>
        /// <param name="classMapping">The class mapping for which to create member mappings.</param>
        /// <returns>A list of <see cref="MemberMapping"/> instances.</returns>
        public static IReadOnlyList<MemberMapping> CreateMemberMappings(ClassMapping classMapping)
        {
            if (classMapping == null) throw new ArgumentNullException(nameof(classMapping));

            var sourceType = classMapping.SourceType;
            var targetType = classMapping.TargetType;

            var sourceProps = sourceType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var targetProps = targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            var mappings = new List<MemberMapping>();
            var mappedPairs = new HashSet<(string, string)>();

            // 1. Attribute-based mappings on source members
            foreach (var sourceMember in sourceType.GetMembers(BindingFlags.Instance | BindingFlags.Public))
            {
                var mapMemberAttrs = sourceMember
                    .GetCustomAttributes(typeof(MapMemberAttribute), inherit: true)
                    .OfType<MapMemberAttribute>()
                    .Where(attr => attr.MappedClassType == targetType)
                    .ToList();

                foreach (var attr in mapMemberAttrs)
                {
                    // Map CopyToSource direction (from mapped class to local class)
                    if (attr.CopyToSource && attr.MappedMemberGetter != null && attr.MappedMemberGetter.DeclaringType == targetType)
                    {
                        var key = (sourceMember.MetadataToken, attr.MappedMemberGetter.MetadataToken);
                        if (!mappedPairs.Contains(key) && AreMembersTypeCompatible(sourceMember, attr.MappedMemberGetter))
                        {
                            mappings.Add(new MemberMapping(classMapping, sourceMember, attr.MappedMemberGetter));
                            mappedPairs.Add(key);
                        }
                    }
                    // Map CopyToTarget direction (from local class to mapped class)
                    if (attr.CopyToTarget && attr.MappedMemberSetter != null && attr.MappedMemberSetter.DeclaringType == targetType)
                    {
                        var key = (sourceMember.MetadataToken, attr.MappedMemberSetter.MetadataToken);
                        if (!mappedPairs.Contains(key) && AreMembersTypeCompatible(sourceMember, attr.MappedMemberSetter))
                        {
                            mappings.Add(new MemberMapping(classMapping, sourceMember, attr.MappedMemberSetter));
                            mappedPairs.Add(key);
                        }
                    }
                }
            }

            // 2. Attribute-based mappings on target members (reverse direction)
            foreach (var targetMember in targetType.GetMembers(BindingFlags.Instance | BindingFlags.Public))
            {
                var mapMemberAttrs = targetMember
                    .GetCustomAttributes(typeof(MapMemberAttribute), inherit: true)
                    .OfType<MapMemberAttribute>()
                    .Where(attr => attr.MappedClassType == sourceType)
                    .ToList();

                foreach (var attr in mapMemberAttrs)
                {
                    // Map CopyToSource direction (from mapped class to local class)
                    if (attr.CopyToSource && attr.MappedMemberGetter != null && attr.MappedMemberGetter.DeclaringType == sourceType)
                    {
                        var key = (attr.MappedMemberGetter.MetadataToken, targetMember.MetadataToken);
                        if (!mappedPairs.Contains(key) && AreMembersTypeCompatible(attr.MappedMemberGetter, targetMember))
                        {
                            mappings.Add(new MemberMapping(classMapping, attr.MappedMemberGetter, targetMember));
                            mappedPairs.Add(key);
                        }
                    }
                    // Map CopyToTarget direction (from local class to mapped class)
                    if (attr.CopyToTarget && attr.MappedMemberSetter != null && attr.MappedMemberSetter.DeclaringType == sourceType)
                    {
                        var key = (attr.MappedMemberSetter.MetadataToken, targetMember.MetadataToken);
                        if (!mappedPairs.Contains(key) && AreMembersTypeCompatible(attr.MappedMemberSetter, targetMember))
                        {
                            mappings.Add(new MemberMapping(classMapping, attr.MappedMemberSetter, targetMember));
                            mappedPairs.Add(key);
                        }
                    }
                }
            }

            // 3. Convention-based mappings (by name and type, not already mapped by attribute)
            foreach (var sourceProp in sourceProps)
            {
                foreach (var targetProp in targetProps)
                {
                    var key = (sourceProp.MetadataToken, targetProp.MetadataToken);
                    if (mappedPairs.Contains(key))
                        continue;

                    if (sourceProp.Name == targetProp.Name &&
                        sourceProp.PropertyType == targetProp.PropertyType &&
                        targetProp.CanWrite && sourceProp.CanRead)
                    {
                        // Already type compatible by definition
                        mappings.Add(new MemberMapping(classMapping, sourceProp, targetProp));
                        mappedPairs.Add(key);
                    }
                }
            }

            return mappings;
        }

        /// <summary>
        /// Checks if two members are type compatible for mapping.
        /// Supports PropertyInfo, FieldInfo, and getter-like MethodInfo.
        /// </summary>
        /// <param name="memberA">First member.</param>
        /// <param name="memberB">Second member.</param>
        /// <returns>True if types are compatible, otherwise false.</returns>
        private static bool AreMembersTypeCompatible(MemberInfo memberA, MemberInfo memberB)
        {
            Type? typeA = memberA.GetMemberDataType();
            Type? typeB = memberB.GetMemberDataType();

            // Use extension for method check
            if (memberA.MemberIsMethod() && !IsValidGetterMethod((MethodInfo)memberA))
                return false;
            if (memberB.MemberIsMethod() && !IsValidGetterMethod((MethodInfo)memberB))
                return false;

            return typeA != null && typeB != null && typeA == typeB;
        }

        /// <summary>
        /// Determines if a method is a valid getter (public, no required parameters, non-void).
        /// </summary>
        /// <param name="method">The method info.</param>
        /// <returns>True if the method is a valid getter, otherwise false.</returns>
        private static bool IsValidGetterMethod(MethodInfo method)
        {
            if (!method.IsPublic)
                return false;
            if (method.ReturnType == typeof(void))
                return false;
            var parameters = method.GetParameters();
            return parameters.Length == 0 || parameters.All(p => p.IsOptional);
        }

        /// <inheritdoc/>
        public bool Equals(ClassMapping? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            return SourceType == other.SourceType
                && TargetType == other.TargetType
                && OutputNamespaceName == other.OutputNamespaceName
                && OutputStaticClassName == other.OutputStaticClassName
                && OutputRegionName == other.OutputRegionName
                && CopyToTargetArgType == other.CopyToTargetArgType;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => Equals(obj as ClassMapping);

        /// <inheritdoc/>
        public override int GetHashCode() => _hashCode;

        /// <inheritdoc/>
        public bool Equals(ClassMapping? x, ClassMapping? y) => x?.Equals(y) ?? y is null;

        /// <inheritdoc/>
        public int GetHashCode(ClassMapping obj) => obj._hashCode;

        /// <summary>
        /// Calculates the hash code for this instance.
        /// </summary>
        /// <returns>The calculated hash code.</returns>
        private int CalculateHashCode()
        {
            return HashCode.Combine(
                SourceType,
                TargetType,
                OutputNamespaceName,
                OutputStaticClassName,
                OutputRegionName,
                CopyToTargetArgType
            );
        }
    }

    /// <summary>
    /// Represents a mapping between a source member and a target member for a given class mapping.
    /// </summary>
    public sealed class MemberMapping : IEquatable<MemberMapping>, IEqualityComparer<MemberMapping>
    {
        /// <summary>
        /// Gets the parent class mapping.
        /// </summary>
        public ClassMapping ClassMapping { get; }

        /// <summary>
        /// Gets the source member info.
        /// </summary>
        public MemberInfo SourceMember { get; }

        /// <summary>
        /// Gets the target member info.
        /// </summary>
        public MemberInfo TargetMember { get; }

        /// <summary>
        /// Gets the region name for the generated code.
        /// </summary>
        public string? OutputRegionName { get; }

        private readonly int _hashCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberMapping"/> class.
        /// </summary>
        /// <param name="classMapping">The parent class mapping.</param>
        /// <param name="sourceMember">The source member info.</param>
        /// <param name="targetMember">The target member info.</param>
        /// <param name="outputRegionName">The region name for the generated code.</param>
        public MemberMapping(
            ClassMapping classMapping,
            MemberInfo sourceMember,
            MemberInfo targetMember,
            string? outputRegionName = null)
        {
            ClassMapping = classMapping ?? throw new ArgumentNullException(nameof(classMapping));
            SourceMember = sourceMember ?? throw new ArgumentNullException(nameof(sourceMember));
            TargetMember = targetMember ?? throw new ArgumentNullException(nameof(targetMember));
            OutputRegionName = outputRegionName;

            _hashCode = CalculateHashCode();
        }

        /// <inheritdoc/>
        public bool Equals(MemberMapping? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            return ClassMapping.Equals(other.ClassMapping)
                && SourceMember.Equals(other.SourceMember)
                && TargetMember.Equals(other.TargetMember)
                && OutputRegionName == other.OutputRegionName;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => Equals(obj as MemberMapping);

        /// <inheritdoc/>
        public override int GetHashCode() => _hashCode;

        /// <inheritdoc/>
        public bool Equals(MemberMapping? x, MemberMapping? y) => x?.Equals(y) ?? y is null;

        /// <inheritdoc/>
        public int GetHashCode(MemberMapping obj) => obj._hashCode;

        /// <summary>
        /// Calculates the hash code for this instance.
        /// </summary>
        /// <returns>The calculated hash code.</returns>
        private int CalculateHashCode()
        {
            return HashCode.Combine(
                ClassMapping,
                SourceMember,
                TargetMember,
                OutputRegionName
            );
        }
    }
}
