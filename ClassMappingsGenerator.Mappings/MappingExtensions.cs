using vsc.ClassMappingsGenerator.Attributes;

namespace vsc.ClassMappingsGenerator.Mappings
{

    public static class MappingExtensions
    { 
        public static IEnumerable<ClassMapping> GetMappings(this Type, MapAttribute mapAttribute)
        {

            new ClassMapping(mapAttribute.MappedClassType, mapAttribute);

            var mapFrom = dtos
                    .SelectMany(
                        dto => dto.GetCustomAttributes(typeof(MapAttribute), true)
                        , (dto, attr) => (dto, attr)
                    )
                    .Distinct()
                    .SelectMany(
                        i => i.attr.MapDescendantDTOs ? types.WhereInheritsFrom(i.dto, includeBaseType: true) : [i.dto]
                        , (i, dto) => (dto, i.attr)
                    )
                    .SelectMany(
                        i => i.attr.MapDescendantClasses ? types.WhereInheritsFrom(i.attr.MappedType, includeBaseType: true) : [i.attr.MappedType]
                        , (i, mappedType) => (i.dto, mappedType, i.attr)
                    )
                    .Distinct()
                    //.SelectMany(
                    //    t => t.GetCustomAttributes(typeof(MapAttribute<,,>), true)
                    //    , (dto, attr) => (dto, mapDescendantClasses: ((IMapAttribute)attr).MapDescendantClasses, type: ((IMapAttribute)attr).MappedType)
                    //)
                    //.Select(i => (i.dto, types: i.mapDescendantClasses ? types.WhereInheritsFrom(i.type, includeBaseType: true) : [i.type]))
                    //.SelectMany(p => p.types, (p, type) => (p.dto, type))
                    .Distinct()
                    .Select(p => new PairMapping(p.dto, p.mappedType, p.attr))
                    .OrderBy(p => p);

            return AssociatePairs(mapFrom).ToArray();
        }
    }
}
