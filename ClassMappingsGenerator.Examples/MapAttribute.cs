using vsc.ClassMappingsGenerator.Attributes;

namespace vsc.ClassMappingsGenerator.Examples.MapAttribute
{
    public class MapAttributeExample { }

    [Map<MapAttributeExample>()]
    public class MapAttributeExample2DTO { }

    [Map<MapAttributeExample>(mapLocalClassDescendants: true, mapMappedClassDescendants:true
        , outputNamespaceName: "vsc.ClassMappingsGenerator.Examples.MapAttribute.Generated",
        outputStaticClassName: "GeneratedMapAttributeExamples"
        , outputRegionName:$"{nameof(MapAttributeExample3DTO)}")]
    public class MapAttributeExample3DTO { }
}
