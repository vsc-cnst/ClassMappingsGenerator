
using System.Reflection;

namespace vsc.ClassMappingsGenerator.Attributes
{
    [Flags]
    public enum MapMemberAction
    {
        /// <summary>
        /// Gets the value from Model Class <see cref="MemberInfo">member</see> and sets <see cref="iDTO"/> <see cref="MemberInfo">member</see>
        /// </summary>
        CopyToDto = 1 << 2,

        /// <summary>
        /// Sets the value from Model Class <see cref="MemberInfo">member</see> and sets <see cref="iDTO"/> <see cref="MemberInfo">member</see>
        /// </summary>
        CopyToClass = 1 << 3,
    }

    public interface IMapMemberAttribute
    {
        /// <summary>
        /// #region e.g. "DTO > Model > PropertyGroup"
        /// </summary>
        public string RegionName { get; }

        /// <summary>
        /// Model Class type to be mapped
        /// </summary>
        public Type MappedClassType { get; }
        /// <summary>
        /// Mapper member getter. Model Class member to get value from.
        /// </summary>
        public MemberInfo? MappedMemberGetter { get; }
        /// <summary>
        /// Mapper member setter. Model Class member to set value to.
        /// </summary>
        public MemberInfo? MappedMemberSetter { get; }

        /// <summary>
        /// Comment to add to the mapping.
        /// </summary>
        public string? Comment { get; }

        /// <summary>
        /// If the mapping is to copy to <see cref="iDTO"/>, to Model Class or both.
        /// </summary>

        /// <summary>
        /// If <see cref="MapAction"/> has <see cref="MapMemberAction.CopyToDto"/> flag.
        /// </summary>public MapMemberAction MapAction { get; }
        public bool CopyToDTO { get; }

        /// <summary>
        /// If <see cref="MapAction"/> has <see cref="MapMemberAction.CopyToClass"/> flag.
        /// </summary>
        public bool CopyToClass { get; }
    }

    /// <summary>
    /// Attribute to map a member of a Model Class to a member of a <see cref="iDTO"/>
    /// </summary>
    /// <typeparam name="TClassType"></typeparam>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Delegate | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class MapMemberAttribute<TClassType> : Attribute, IMapMemberAttribute
    {
        public string? RegionName { get; }

        public Type MappedClassType { get; }

        public MemberInfo? MappedMemberGetter { get; }

        public MemberInfo? MappedMemberSetter { get; }
        public string? Comment { get; }

        public MapMemberAction MapAction { get; }

        public bool CopyToDTO => MapAction.HasFlag(MapMemberAction.CopyToDto);

        public bool CopyToClass => MapAction.HasFlag(MapMemberAction.CopyToClass);

        /// <summary>
        /// Create a mapping between a <see cref="iDTO"/> member and a Model Class member.
        /// </summary>
        /// <param name="mappedMemberName">
        /// Name of the member in the Model Class to map to.
        /// </param>
        /// <param name="action">
        /// Whether to copy the value from the Model Class to the <see cref="iDTO"/>, from the <see cref="iDTO"/> to the Model Class or both.
        /// </param>
        /// <param name="comment">
        /// Comment to add to the mapping.
        /// </param>
        public MapMemberAttribute(string mappedMemberName, MapMemberAction action = MapMemberAction.CopyToDto | MapMemberAction.CopyToClass, string? comment = null, string[]? regions = null)
        {
            RegionName = regions is null ? null : string.Join(" > ", regions.Where(r => !string.IsNullOrEmpty(r)));

            MappedClassType = typeof(TClassType);
            MapAction = action;

            if (CopyToDTO)
                MappedMemberGetter = typeof(TClassType).GetMember(mappedMemberName).Single();

            if (CopyToClass)
                MappedMemberSetter = typeof(TClassType).GetMember(mappedMemberName).Single();

            Comment = comment;
        }

        /// <summary>
        /// Create a mapping between a <see cref="iDTO"/> <see cref="MemberInfo">member</see> and a Model Class <see cref="MemberInfo">member</see>.
        /// <br/>
        /// Allows for different getter and setter.
        /// </summary>
        /// <param name="getterMemberName">
        /// Name of the <see cref="MemberInfo">member</see> in the Model Class to get value <b>from</b>.
        /// </param>
        /// <param name="setterMemberName">
        /// Name of the <see cref="MemberInfo">member</see> in the Model Class to set value <b>to</b>.
        /// </param>
        /// <param name="comment">
        /// Comment to add to the mapping.
        /// </param>
        public MapMemberAttribute(string getterMemberName, string setterMemberName, string? comment = null)
        {
            MapAction = MapMemberAction.CopyToDto | MapMemberAction.CopyToClass;

            MappedClassType = typeof(TClassType);
            MappedMemberGetter = typeof(TClassType).GetMember(getterMemberName).Single();
            MappedMemberSetter = typeof(TClassType).GetMember(setterMemberName).Single();

            Comment = comment;
        }
    }

    /// <summary>
    /// <b>Attribute for Single Direction Mapping</b>. Create a mapping <b>from</b> a Model Class <see cref="MemberInfo">member</see> <b>to</b> a <see cref="iDTO"/> <see cref="MemberInfo">member</see>.
    /// </summary>
    /// <typeparam name="TClassType"></typeparam>
    /// <param name="mappedMemberName"></param>
    /// <param name="comment"></param>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Delegate | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class MapFromMemberAttribute<TClassType>(string mappedMemberName, string? comment = null, string[]? regions = null)
        : MapMemberAttribute<TClassType>(mappedMemberName, MapMemberAction.CopyToDto, comment, regions)
    {
    }


    /// <summary>
    /// <b>Attribute for Single Direction Mapping</b>. Create a mapping <b>to</b> a Model Class <see cref="MemberInfo">member</see> <b>from</b> a <see cref="iDTO"/> <see cref="MemberInfo">member</see>.
    /// </summary>
    /// <typeparam name="TClassType"></typeparam>
    /// <param name="mappedMemberName">
    /// Name of the member in the <b>Model Class</b> to map <b>to</b>.
    /// </param>
    /// <param name="comment">
    /// Comment to add to the mapping.
    /// </param>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Delegate | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class MapToMemberAttribute<TClassType>(string mappedMemberName, string? comment = null, string[]? region = null)
        : MapMemberAttribute<TClassType>(mappedMemberName, MapMemberAction.CopyToClass, comment, region)
    {

    }
}
