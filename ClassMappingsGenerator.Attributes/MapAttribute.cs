
using System.Diagnostics.CodeAnalysis;

namespace vsc.ClassMappingsGenerator.Attributes
{

    /// <summary>
    /// Marks two classes that have properties that represent the same data, 
    /// </summary>
    /// 
    /// <remarks>
    ///     <para>
    ///         <b>Purpose:</b><br/>
    ///         Allowing automatic generation of property-copying code between them.
    ///     </para>
    ///     <para>
    ///         <b>Terminology:</b><br/>
    ///         <b>LocalClass</b>: The class where the attribute is applied.<br/>
    ///         <b>MappedClass</b>: The target class being mapped to.
    ///     </para>
    /// </remarks>
    /// 
    /// <example>
    /// <b>Usage Example:</b>
    /// <code>
    /// public partial class MappedClassExample {}
    ///
    /// [Map&lt;Animal&gt;] // Preferred usage
    /// public partial class LocalClassExample {}
    /// </code>
    /// </example>
    /// <param name="MappedClassType">Type of the class to map with</param>
    /// <param name="mapLocalClassDescendants">Wheather to also map <b>LocalClass</b> descendant types with <see cref="MappedClass">MappedClass</see></param>
    /// <param name="mapMappedClassDescendants">Wheather to also map <see cref="MappedClass">MappedClass</see> descendant types with <b>LocalClass</b></param>
    /// <param name="outputNamespaceName">Namespace where to output generated classes</param>
    /// <param name="outputStaticClassName">Static class where to output generated methods</param>
    /// <param name="outputRegionName">Region inside the <see cref="outputStaticClassName"/> where to output generated methods</param>
    /// <param name="toLocalClassArgsType" >Type for mathods aditional parameter when copying to LocalClass type (defaults to <see cref="IParamArgs"/>)</param>
    /// <param name="toMappedClassArgsType">Type for mathods aditional parameter when copying to <see cref="MappedClassType">MappedClass</see> type (defaults to <see cref="IParamArgs"/>)</param>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public partial class MapAttribute
    (
        Type MappedClassType,
        bool mapLocalClassDescendants = false, bool mapMappedClassDescendants = false,
        string? outputNamespaceName = null, string? outputStaticClassName = null, string? outputRegionName = null,
        Type? toLocalClassArgsType = null, Type? toMappedClassArgsType = null
    ) : Attribute
    {
        public Type MappedClassType { get; } = MappedClassType;

        public bool MapLocalClassDescendants { get; } = mapMappedClassDescendants;
        public bool MappedClassClasssDescendants { get; } = mapLocalClassDescendants;

        #region Generation Params
        public string? OutputNamespaceName { get; } = outputNamespaceName;
        public string? OutputStaticClassName { get; } = outputStaticClassName;
        public string? OutputRegionName { get; } = outputRegionName;
        public virtual Type? ToLocalClassArgsType => toLocalClassArgsType ?? typeof(ICopyToLocalTypeArgType);
        public virtual Type? ToMappedClassArgsType => toMappedClassArgsType ?? typeof(ICopyToMappedTypeArgType);
        #endregion Generation Params

    }

    /// <inheritdoc/>
    /// 
    /// <typeparam name="TMappedClassType">Type of the class to map with</typeparam>
    /// <param name="mapLocalClassDescendants">Wheather to also map <b>LocalClass</b> descendant types with <see cref="MappedClass">MappedClass</see></param>
    /// <param name="mapMappedClassDescendants">Wheather to also map <see cref="MappedClass">MappedClass</see> descendant types with <b>LocalClass</b></param>
    /// <param name="outputNamespaceName">Namespace where to output generated classes</param>
    /// <param name="outputStaticClassName">Static class where to output generated methods</param>
    /// <param name="outputRegionName">Region where to output generated methods</param>
    /// <param name="toLocalClassArgsType">Type for mathods aditional parameter when copying to LocalClass type (defaults to <see cref="IParamArgs"/>)</param>
    /// <param name="toTargetClassArgsType">Type for mathods aditional parameter when copying to <see cref="MappedClassType">MappedClass</see> type (defaults to <see cref="IParamArgs"/>)</param>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public partial class MapAttribute<TMappedClassType>(bool mapLocalClassDescendants = false,
        bool mapMappedClassDescendants = false, string? outputNamespaceName = null, string? outputStaticClassName = null, string? outputRegionName = null,
        Type? toLocalClassArgsType = null, Type? toTargetClassArgsType = null
    ) : MapAttribute(
            typeof(TMappedClassType), 
            mapLocalClassDescendants: mapLocalClassDescendants, 
            mapMappedClassDescendants,
            outputNamespaceName, outputStaticClassName, outputRegionName, 
            toLocalClassArgsType, toTargetClassArgsType)
    {
    }

}
